#nullable enable

using OnlyChain.Database;
using OnlyChain.Model;
using OnlyChain.Secp256k1;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    [DebuggerDisplay("{db}")]
    unsafe public class BlockChainDatabase : IDisposable {
        enum KeyType : byte {
            // k: 区块高度（8字节大端序）
            // v: 区块hash
            BlockHash = 0,
            // k: 区块hash
            // v: 区块头部序列化
            BlockHeader = 1,
            // k: 区块hash
            // v: 交易hash数组
            BlockBody = 2,
            // k: 交易hash
            // v: 交易序列化, 交易执行结果
            Transaction = 3,
            // k: 地址, 序号（8字节大端序）
            // v: 交易hash
            /// <summary>
            /// 某地址的所有交易发送记录
            /// </summary>
            TransactionSend = 4,
            // k: 地址,序号（8字节大端序）
            // v: 交易hash
            /// <summary>
            /// 某地址的所有交易接收记录
            /// </summary>
            TransactionReceive = 5,
            // k: 地址
            // v: 公钥
            /// <summary>
            /// 所有具有公钥的账户
            /// </summary>
            ConfirmedUser = 6,
            // k: 地址
            // v: (null)
            /// <summary>
            /// 所有暂时不能获得公钥的账户（通常是收款地址）
            /// </summary>
            UnconfirmedUser = 7,
        }

        static readonly LevelDBComparator comparator = new LevelDBComparator("onlychain", (k1, k2) => {
            if (k1.Length > 0 && k2.Length > 0) {
                int typeDiff = k1[0] - k2[0];
                if (typeDiff != 0) return typeDiff;
                return k1[1..].SequenceCompareTo(k2[1..]);
            }
            return k1.SequenceCompareTo(k2);
        });


        private readonly LevelDB db;

        internal LevelDB DB => db;


        public BlockChainDatabase(string path) {
            var dirPath = Path.GetDirectoryName(path) ?? throw new ArgumentException("错误的路径", nameof(path));
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            var options = new LevelDBOptions {
                CreateIfMissing = true,
                Cache = new LevelDBCache(10000),
                Comparator = comparator,
            };

            db = new LevelDB(path, options);
        }

        public void PutBlock(Block block) {
            if (GetBlockHash(block.Height) != Bytes<Hash256>.Empty) return;

            using var writeBatch = new LevelDBWriteBatch();
            PutBlockHash(writeBatch, block.Height, block.Hash);
            PutBlockHeader(writeBatch, block.Hash, block.SerializeHeader());

            Bytes<Hash256>[] txHashes = Array.ConvertAll(block.Transactions, t => t.Hash);
            PutBlockBody(writeBatch, block.Hash, txHashes);

            var maxSendIndexes = new Dictionary<Bytes<Address>, ulong>();
            var maxRecvIndexes = new Dictionary<Bytes<Address>, ulong>();

            var users = new Dictionary<Bytes<Address>, PublicKey?>();
            if (block.ProducerAddress != Bytes<Address>.Empty) {
                users.Add(block.ProducerAddress, block.ProducerPublicKey);
            }

            foreach (Transaction tx in block.Transactions) {
                PutTransaction(writeBatch, tx.Hash, tx.NetworkSerialize());
                LocalPutUser(users, tx.From, tx.FromPublicKey);
                LocalPutUser(users, tx.To, null);

                if (!maxSendIndexes.TryGetValue(tx.From, out ulong maxSendIndex) && !GetTransactionMaxSendIndex(tx.From, out maxSendIndex)) {
                    maxSendIndex = 0;
                    PutTransactionSend(writeBatch, tx.From, ulong.MaxValue, default);
                } else {
                    maxSendIndex++;
                }

                PutTransactionSend(writeBatch, tx.From, maxSendIndex, tx.Hash);
                maxSendIndexes[tx.From] = maxSendIndex;

                if (!maxRecvIndexes.TryGetValue(tx.To, out ulong maxRecvIndex) && !GetTransactionMaxRecvIndex(tx.To, out maxRecvIndex)) {
                    maxRecvIndex = 0;
                    PutTransactionReceive(writeBatch, tx.To, ulong.MaxValue, default);
                } else {
                    maxRecvIndex++;
                }

                PutTransactionReceive(writeBatch, tx.To, maxRecvIndex, tx.Hash);
                maxRecvIndexes[tx.To] = maxRecvIndex;
            }

            PutUsers(writeBatch, users);

            db.Write(writeBatch, sync: false);



            static void LocalPutUser(Dictionary<Bytes<Address>, PublicKey?> users, Bytes<Address> address, PublicKey? publicKey) {
                if (users.TryGetValue(address, out var value) && value is not null) return;
                users[address] = publicKey;
            }
        }

        /// <summary>
        /// 创世区块专用
        /// </summary>
        /// <param name="users"></param>
        public void PutUsers(IReadOnlyDictionary<Bytes<Address>, PublicKey?> users) {
            using var writeBatch = new LevelDBWriteBatch();
            PutUsers(writeBatch, users);
            db.Write(writeBatch, sync: false);
        }

        private void PutUsers(LevelDBWriteBatch writeBatch, IReadOnlyDictionary<Bytes<Address>, PublicKey?> users) {
            uint unconfirmedCount = GetUnconfirmedUserCount();
            uint confirmedCount = GetConfirmedUserCount();

            foreach (var (addr, publicKey) in users) {
                PutUser(writeBatch, addr, publicKey, ref unconfirmedCount, ref confirmedCount);
            }

            PutUserCount(writeBatch, KeyType.UnconfirmedUser, unconfirmedCount);
            PutUserCount(writeBatch, KeyType.ConfirmedUser, confirmedCount);
        }

        public Block? GetBlock(uint blockHeight) {
            var hash = GetBlockHash(blockHeight);
            if (hash == Bytes<Hash256>.Empty) return null;
            return GetBlock(hash) ?? throw new BadDatabaseException();
        }

        public Block? GetBlock(Bytes<Hash256> blockHash) {
            try {
                if (GetBlockHeader(blockHash) is not byte[] header) return null;

                Bytes<Hash256>[] txHashes = GetBlockBody(blockHash) ?? throw new BadDatabaseException();

                Block block = new Block(header, hasTx: false) {
                    Transactions = Array.ConvertAll(txHashes, hash => GetTransaction(hash) ?? throw new BadDatabaseException())
                };
                return block;
            } catch (BadDatabaseException) {
                throw;
            } catch {
                throw new BadDatabaseException();
            }
        }

        public Transaction? GetTransaction(Bytes<Hash256> txHash) {
            if (GetTransactionRawData(txHash) is not byte[] rawData) return null;
            return Transaction.NetworkDeserialize(rawData);
        }

        private static Span<byte> MakeKey(Span<byte> key, KeyType type, ulong index) {
            Debug.Assert(key.Length == 1 + sizeof(ulong));

            key[0] = (byte)type;
            BinaryPrimitives.WriteUInt64BigEndian(key[1..], index);
            return key;
        }

        private static Span<byte> MakeKey<T>(Span<byte> key, KeyType type, Bytes<T> hash) where T : unmanaged {
            Debug.Assert(key.Length == 1 + sizeof(T));

            key[0] = (byte)type;
            new ReadOnlySpan<byte>(&hash, sizeof(T)).CopyTo(key[1..]);
            return key;
        }

        private static Span<byte> MakeKey<T>(Span<byte> key, KeyType type, Bytes<T> hash, ulong index) where T : unmanaged {
            Debug.Assert(key.Length == 1 + sizeof(T) + sizeof(ulong));

            key[0] = (byte)type;
            new ReadOnlySpan<byte>(&hash, sizeof(T)).CopyTo(key[1..]);
            BinaryPrimitives.WriteUInt64BigEndian(key[(1 + sizeof(T))..], index);
            return key;
        }

        [SkipLocalsInit]
        public Bytes<Hash256> GetBlockHash(uint height) {
            Span<byte> key = MakeKey(stackalloc byte[9], KeyType.BlockHash, height);
            if (db.Get(key, out Bytes<Hash256> value)) {
                return value;
            } else {
                return Bytes<Hash256>.Empty;
            }
        }

        [SkipLocalsInit]
        public byte[]? GetBlockHeader(Bytes<Hash256> blockHash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.BlockHeader, blockHash);
            return db.Get(key);
        }

        [SkipLocalsInit]
        public Bytes<Hash256>[]? GetBlockBody(Bytes<Hash256> blockHash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.BlockBody, blockHash);
            return db.Get<Bytes<Hash256>>(key);
        }

        [SkipLocalsInit]
        public byte[]? GetTransactionRawData(Bytes<Hash256> txHash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.Transaction, txHash);
            return db.Get(key);
        }

        [SkipLocalsInit]
        private bool GetTransactionMaxIndex(KeyType type, Bytes<Address> address, [MaybeNullWhen(false)] out ulong maxIndex) {
            Span<byte> maxKey = MakeKey(stackalloc byte[1 + sizeof(Address) + sizeof(ulong)], type, address, ulong.MaxValue);
            using var iterator = new LevelDBIterator(db, LevelDBReadOptions.Default);
            iterator.Seek(maxKey);
            if (!iterator.IsValid) {
                maxIndex = 0;
                return false;
            }
            iterator.Previous();
            try {
                maxIndex = BinaryPrimitives.ReadUInt64BigEndian(iterator.Key[(1 + sizeof(Address))..]);
                return true;
            } catch {
                throw new BadDatabaseException();
            }
        }

        public bool GetTransactionMaxSendIndex(Bytes<Address> address, [MaybeNullWhen(false)] out ulong maxIndex) {
            return GetTransactionMaxIndex(KeyType.TransactionSend, address, out maxIndex);
        }

        public bool GetTransactionMaxRecvIndex(Bytes<Address> address, [MaybeNullWhen(false)] out ulong maxIndex) {
            return GetTransactionMaxIndex(KeyType.TransactionReceive, address, out maxIndex);
        }

        [SkipLocalsInit]
        public Bytes<Hash256> GetTransactionSend(Bytes<Address> address, ulong index) {
            if (!GetTransactionMaxSendIndex(address, out ulong maxIndex) || index > maxIndex) return Bytes<Hash256>.Empty;

            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address) + sizeof(ulong)], KeyType.TransactionSend, address, index);
            if (db.Get(key, out Bytes<Hash256> value)) {
                return value;
            } else {
                throw new BadDatabaseException();
            }
        }

        [SkipLocalsInit]
        public Bytes<Hash256> GetTransactionReceive(Bytes<Address> address, ulong index) {
            if (!GetTransactionMaxRecvIndex(address, out ulong maxIndex) || index > maxIndex) return Bytes<Hash256>.Empty;

            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address) + sizeof(ulong)], KeyType.TransactionReceive, address, index);
            if (db.Get(key, out Bytes<Hash256> value)) {
                return value;
            } else {
                throw new BadDatabaseException();
            }
        }

        [SkipLocalsInit]
        private uint GetUserCount(KeyType type) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], type, Bytes<Address>.Max);
            Span<byte> value = stackalloc byte[sizeof(uint)];
            if (db.Get(key, value)) {
                return BinaryPrimitives.ReadUInt32BigEndian(value);
            } else {
                return 0;
            }
        }

        public uint GetConfirmedUserCount() => GetUserCount(KeyType.ConfirmedUser);

        public uint GetUnconfirmedUserCount() => GetUserCount(KeyType.UnconfirmedUser);

        [SkipLocalsInit]
        public bool GetUnconfirmedUser(Bytes<Address> address) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], KeyType.UnconfirmedUser, address);
            return db.Get(key, Span<byte>.Empty);
        }

        [SkipLocalsInit]
        public PublicKey? GetConfirmedUserPublicKey(Bytes<Address> address) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], KeyType.ConfirmedUser, address);
            Span<byte> value = stackalloc byte[33];
            return db.Get(key, value) ? PublicKey.Parse(value) : null;
        }

        public bool GetUserPublicKey(Bytes<Address> address, out PublicKey? publicKey) {
            publicKey = GetConfirmedUserPublicKey(address);
            if (publicKey is not null) return true;
            return GetUnconfirmedUser(address);
        }


        [SkipLocalsInit]
        private static void PutBlockHash(LevelDBWriteBatch writeBatch, uint height, Bytes<Hash256> hash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(ulong)], KeyType.BlockHash, height);
            writeBatch.Put(key, new ReadOnlySpan<byte>(&hash, sizeof(Hash256)));
        }

        [SkipLocalsInit]
        private static void PutBlockHeader(LevelDBWriteBatch writeBatch, Bytes<Hash256> blockHash, ReadOnlySpan<byte> blockHeader) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.BlockHeader, blockHash);
            writeBatch.Put(key, blockHeader);
        }

        [SkipLocalsInit]
        private static void PutBlockBody(LevelDBWriteBatch writeBatch, Bytes<Hash256> blockHash, ReadOnlySpan<Bytes<Hash256>> txHashes) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.BlockBody, blockHash);
            writeBatch.Put(key, MemoryMarshal.Cast<Bytes<Hash256>, byte>(txHashes));
        }

        [SkipLocalsInit]
        private static void PutTransaction(LevelDBWriteBatch writeBatch, Bytes<Hash256> txHash, ReadOnlySpan<byte> txBytes) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Hash256)], KeyType.Transaction, txHash);
            writeBatch.Put(key, txBytes);
        }

        [SkipLocalsInit]
        private static void PutTransactionSend(LevelDBWriteBatch writeBatch, Bytes<Address> address, ulong index, Bytes<Hash256> txHash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address) + sizeof(ulong)], KeyType.TransactionSend, address, index);
            writeBatch.Put(key, new ReadOnlySpan<byte>(&txHash, sizeof(Hash256)));
        }

        [SkipLocalsInit]
        private static void PutTransactionReceive(LevelDBWriteBatch writeBatch, Bytes<Address> address, ulong index, Bytes<Hash256> txHash) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address) + sizeof(ulong)], KeyType.TransactionReceive, address, index);
            writeBatch.Put(key, new ReadOnlySpan<byte>(&txHash, sizeof(Hash256)));
        }

        [SkipLocalsInit]
        private static void PutUserCount(LevelDBWriteBatch writeBatch, KeyType type, uint count) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], type, Bytes<Address>.Max);
            Span<byte> value = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(value, count);
            writeBatch.Put(key, value);
        }

        [SkipLocalsInit]
        private static void PutUnconfirmedUser(LevelDBWriteBatch writeBatch, Bytes<Address> address) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], KeyType.UnconfirmedUser, address);
            writeBatch.Put(key, ReadOnlySpan<byte>.Empty);
        }

        [SkipLocalsInit]
        private static void PutConfirmedUser(LevelDBWriteBatch writeBatch, Bytes<Address> address, PublicKey publicKey) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], KeyType.ConfirmedUser, address);
            Span<byte> value = stackalloc byte[33];
            publicKey.Serialize(value, compressed: true);
            writeBatch.Put(key, value);
        }

        [SkipLocalsInit]
        private static void DeleteUnconfirmedUser(LevelDBWriteBatch writeBatch, Bytes<Address> address) {
            Span<byte> key = MakeKey(stackalloc byte[1 + sizeof(Address)], KeyType.UnconfirmedUser, address);
            writeBatch.Delete(key);
        }

        private void PutUser(LevelDBWriteBatch writeBatch, Bytes<Address> address, PublicKey? publicKey, ref uint unconfirmedCount, ref uint confirmedCount) {
            if (publicKey is null) {
                if (GetUserPublicKey(address, out _)) return;

                PutUnconfirmedUser(writeBatch, address);
                unconfirmedCount++;
            } else {
                if (GetConfirmedUserPublicKey(address) is not null) return;

                if (GetUnconfirmedUser(address)) {
                    DeleteUnconfirmedUser(writeBatch, address);
                    unconfirmedCount--;
                }

                PutConfirmedUser(writeBatch, address, publicKey);
                confirmedCount++;
            }
        }


        public void Repair() => db.Repair();


        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    db.Dispose();
                }

                disposedValue = true;
            }
        }


        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
