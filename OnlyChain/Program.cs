using OnlyChain.Core;
using OnlyChain.Coding;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using OnlyChain.Network.Objects;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using OnlyChain.Network;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using OnlyChain.Database;
using System.Collections;
using System.Diagnostics;
using OnlyChain.Secp256k1;
using OnlyChain.Model;

namespace OnlyChain {
    class Program {
        static string[] messages;

        static byte[] TestArray => new byte[0];

        sealed class StringHashAlgorithm : MerklePatriciaTree<Bytes<Address>, string, Bytes<Hash256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Bytes<Address>, string, Bytes<Hash256>>.IHashAlgorithm Default = new StringHashAlgorithm();

            private readonly static SHA256 sha256 = SHA256.Create();

            private StringHashAlgorithm() { }

            public Bytes<Hash256> ComputeHash(Bytes<Address> key, in string value) {
                Bytes<Hash256> result = default;
                sha256.TryComputeHash(Encoding.UTF8.GetBytes(value), result.Span, out _);
                return result;
            }

            public Bytes<Hash256> ComputeHash(ReadOnlySpan<Bytes<Hash256>> hashes) {
                Bytes<Hash256> result = default;
                sha256.TryComputeHash(MemoryMarshal.Cast<Bytes<Hash256>, byte>(hashes), result.Span, out _);
                return result;
            }
        }

        unsafe static void TestEC() {
            const int Rows = 1024 * 1024;
            const int DataStride = 15;
            const int ErrorStride = 7;

            var random = new Random();
            var data = new byte[DataStride * Rows];
            var ec = new byte[ErrorStride * Rows];
            random.NextBytes(data);

            ErasureCoding.Encode(data, ec, DataStride, ErrorStride);
            var data2 = data.Clone() as byte[];
            var ec2 = ec.Clone() as byte[];
            ErasureCodingIndex[] indexes = {
                (0, 0), (3, 1), (5, 3), (6, 5), (7, 4), (9, 2)
            };
            for (int i = 0; i < Rows; i++) {
                foreach (var (dataMissIndex, errorIndex) in indexes) {
                    data2[i * DataStride + dataMissIndex] = ec2[i * ErrorStride + errorIndex];
                }
            }
            var dataClone = data.Clone() as byte[];
            ErasureCoding.Decode(data2, DataStride, indexes);
            if (!data.AsSpan().SequenceEqual(data2)) throw new Exception();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Restart();
            for (int i = 0; i < 50; i++) {
                ErasureCoding.Encode(data, ec, DataStride, ErrorStride);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 50; i++) {
                ErasureCoding.Decode(dataClone.Clone() as byte[], DataStride, indexes);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        unsafe static void TestRipemd160() {
            var sw = new System.Diagnostics.Stopwatch();
            const int N = 1000000;
            var hash = Bytes<Hash256>.Empty;

            Bytes<Hash160> result = default;

            sw.Restart();
            for (int i = 0; i < N; i++) {
                result = Ripemd160.ComputeHash(hash);
            }
            sw.Stop();
            Console.WriteLine(new TimeSpan(sw.Elapsed.Ticks / N));
            Console.WriteLine(result.ToString());
        }

        static void TestMPT() {
            var keys = new Bytes<Address>[4];
            for (int i = 0; i < keys.Length; i++) keys[i] = Bytes<Address>.Random();

            var tree = new MerklePatriciaTree<Bytes<Address>, string, Bytes<Hash256>>();
            for (int i = 0; i < keys.Length; i++) {
                tree.Add(keys[i], keys[i].ToString());
            }
            tree.ComputeHash(StringHashAlgorithm.Default);
            var tree2 = tree.NextNew();
            tree2.Remove(keys[0]);
            tree2.ComputeHash(StringHashAlgorithm.Default);
        }

        static void TestHashes() {
            var hashes = new Hashes<Bytes<Address>>("hash160.hashes");
            var hashtable = new Hashtable();
            var keys = new Bytes<Address>[1000_0000];
            for (int i = 0; i < keys.Length; i++) keys[i] = Bytes<Address>.Random();

            var tasks = new Task[16];
            var indexes = new int[keys.Length];
            int startIndex = hashes.Count;

            {
                var sw = new System.Diagnostics.Stopwatch();

                sw.Restart();
                for (int i = 0; i < indexes.Length / 2; i++) {
                    hashtable.Add(keys[i], null);
                }
                sw.Stop();
                Console.WriteLine($"hashtable: {sw.Elapsed}");

                sw.Restart();
                for (int i = 0; i < indexes.Length / 2; i++) {
                    indexes[i] = hashes.Add(keys[i]);
                }
                sw.Stop();
                Console.WriteLine($"hashes: {sw.Elapsed}");

                //    sw.Restart();
                //    for (int i = indexes.Length / 2; i < indexes.Length; i++) {
                //        if (hashes.GetIndex(keys[i]) >= 0) throw new Exception();
                //    }
                //    sw.Stop();
                //    Console.WriteLine($"{sw.Elapsed}");
                //    return;
            }

            for (int n = 0; n < tasks.Length; n++) {
                int taskId = n;
                tasks[n] = Task.Run(() => {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Restart();
                    for (int i = 0; i < indexes.Length / 2; i++) {
                        indexes[i] = hashes.GetIndex(keys[i]);
                        if (indexes[i] != i + startIndex) throw new Exception();
                    }
                    sw.Stop();
                    Console.WriteLine($"{taskId}, {sw.Elapsed}");
                });
            }
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Restart();
                for (int i = indexes.Length / 2; i < indexes.Length; i++) {
                    indexes[i] = hashes.Add(keys[i]);
                    // if (hashes.GetIndex(keys[i]) >= 0) throw new Exception();
                }
                sw.Stop();
                Console.WriteLine($"hashes: {sw.Elapsed}");
                Console.WriteLine(hashes.Count);
            }
            Task.WaitAll(tasks);

            Console.WriteLine("=================");

            for (int n = 0; n < tasks.Length; n++) {
                int taskId = n;
                tasks[n] = Task.Run(() => {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Restart();
                    for (int i = 0; i < indexes.Length / 2; i++) {
                        indexes[i] = hashes.GetIndex(keys[i]);
                        if (indexes[i] != i + startIndex) throw new Exception();
                    }
                    sw.Stop();
                    Console.WriteLine($"{taskId}, {sw.Elapsed}");
                });
            }

            Task.WaitAll(tasks);
        }

        static void TestSecp256k1() {
            const int N = 10000;

            TimeSpan privKeyTime = default, publicKeyTime = default, signTime = default, verifyTime = default, recoverTime = default;
            var sw = new Stopwatch();

            Span<byte> msg = stackalloc byte[32];
            new Random().NextBytes(msg);

            for (int i = 0; i < N; i++) {
                sw.Restart();
                var privKey = Secp256k1.Secp256k1.CreatePrivateKey();
                sw.Stop();
                privKeyTime += sw.Elapsed;

                sw.Restart();
                var publicKey = Secp256k1.Secp256k1.CreatePublicKey(privKey);
                sw.Stop();
                publicKeyTime += sw.Elapsed;

                sw.Restart();
                var sign = Secp256k1.Secp256k1.Sign(privKey, msg);
                sw.Stop();
                signTime += sw.Elapsed;

                sw.Restart();
                bool result = Secp256k1.Secp256k1.Verify(publicKey, msg, sign);
                sw.Stop();
                verifyTime += sw.Elapsed;

                if (result is false) throw new Exception("验证失败");

                sw.Restart();
                var publicKey2 = Secp256k1.Secp256k1.RecoverPublicKey(msg, sign);
                sw.Stop();
                recoverTime += sw.Elapsed;

                if (publicKey2.X != publicKey.X || publicKey2.Y != publicKey.Y) throw new Exception("恢复失败");
            }

            Console.WriteLine($"单次创建私钥耗时：{privKeyTime / N}");
            Console.WriteLine($"单次创建公钥耗时：{publicKeyTime / N}");
            Console.WriteLine($"单次签名耗时：{signTime / N}");
            Console.WriteLine($"单次验证耗时：{verifyTime / N}");
            Console.WriteLine($"单次恢复公钥耗时：{recoverTime / N}");
        }

        static void TestDB() {
            LevelDB db = new LevelDB("test", new LevelDBOptions { CreateIfMissing = true });
            db.Put(new byte[] { 2 }, new byte[] { 1, 2 }, sync: true);
            db.Put(new byte[] { 3 }, new byte[] { 1, 3 }, sync: true);
            db.Put(new byte[] { 1 }, new byte[] { 1, 1 }, sync: true);
            LevelDBIterator iterator = new LevelDBIterator(db, LevelDBReadOptions.Default);
            iterator.Seek(new byte[] { 3 });
            iterator.Previous();
        }

        static void TestBlockChip() {
            Bytes<Hash256> prevHash = Bytes<Hash256>.Random();
            Bytes<Hash256> mptHash = Bytes<Hash256>.Random();
            Bytes<Hash256> txHash = Bytes<Hash256>.Random();

            byte[] privKey = Secp256k1.Secp256k1.CreatePrivateKey();
            PublicKey publicKey = Secp256k1.Secp256k1.CreatePublicKey(privKey);
            Bytes<Address> address = publicKey.ToAddress();

            Block block = new Block() {
                Version = 1,
                Height = 2,
                Timestamp = 4,
                HashPrevBlock = prevHash,
                HashWorldState = mptHash,
                HashTxMerkleRoot = txHash,
                ProducerPublicKey = publicKey,
            };
            Serializer serializer = new Serializer();
            block.NetworkSerializeHeaderWithoutSignature(ref serializer);
            block.HashSignHeader = serializer.RawData.MessageHash();
            block.Signature = Ecdsa.Sign(privKey, block.HashSignHeader);
            serializer.WriteSignature(block.Signature);
            block.Hash = serializer.RawData.MessageHash();
            block.Transactions = Array.Empty<Model.Transaction>();

            var chips = BlockChip.Split(block, 20, 14, privKey);
            BlockChipCollection chipCollection = new BlockChipCollection(chips[5], address, publicKey);
            for (int i = 0; i < chips.Length; i++) {
                switch (i) {
                    case 5 or 6 or 7 or 8: continue;
                }

                chipCollection.Add(chips[i]);
            }
            var block2 = chipCollection.RestoreAsync().Result;
            Debug.Assert(block.Hash == block2.Hash);
        }

        private static void TestAsyncDB() {
            //var random = new Random();
            //LevelDBWriteBatch writeBatch = new LevelDBWriteBatch();
            //LevelDB nativeDB = new LevelDB(@"E:\leveldb_test", new LevelDBOptions { CreateIfMissing = true });

            ////for (int i = 0; i < 10000; i++) {
            ////    var key = new byte[100];
            ////    random.NextBytes(key);
            ////    writeBatch.Put(key, key);
            ////}
            ////nativeDB.Write(writeBatch, true);

            //AsyncLevelDB db = new AsyncLevelDB(nativeDB);

            //var tasks = new Task[10000];
            //Stopwatch sw = new Stopwatch();

            //sw.Restart();
            //for (int i = 0; i < tasks.Length; i++) {
            //    var key = new byte[100];
            //    random.NextBytes(key);
            //    tasks[i] = Task.Run(delegate {
            //        nativeDB.Get(key);
            //    });
            //}
            //Task.WaitAll(tasks);
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            //sw.Restart();
            //for (int i = 0; i < tasks.Length; i++) {
            //    var key = new byte[100];
            //    random.NextBytes(key);
            //    tasks[i] = db.Get(key).AsTask();
            //}
            //Task.WaitAll(tasks);
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            //sw.Restart();
            //for (int i = 0; i < tasks.Length; i++) {
            //    tasks[i] = Task.Run(delegate {
            //        Thread.Sleep(1);
            //    });
            //}
            //Task.WaitAll(tasks);
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
        }

        static void TestBlockDB() {
            //for (int i = 0; i < 16; i++) {
            //    new Task(delegate {
            //        while (true) {
            //            var privkey = Secp256k1.Secp256k1.CreatePrivateKey();
            //            var pubkey = Secp256k1.Secp256k1.CreatePublicKey(privkey);
            //            var addr = pubkey.ToAddress();
            //            if (addr.ReadOnlySpan.StartsWith(new byte[] { 0x23, 0x33, 0x33 })) {
            //                Console.WriteLine(Hex.ToString(privkey));
            //            }
            //        }
            //    }, TaskCreationOptions.LongRunning).Start();
            //}
            //Task.Delay(-1).Wait();

            //Block testBlock = BlockChainSystem.BuildGenesisBlock();
            //var db = new BlockChainDatabase(@"Z:\test");
            //db.PutBlock(testBlock);
            //var outBlock = db.GetBlock(1);

            var addresses = Config.Users.Take(20).Select(p => p.Address).ToArray();
            var dbs = addresses.Select(addr => new BlockChainDatabase(Path.Combine("Z:", $"{addr}-main", "blocks"))).ToArray();
            var b1 = dbs[0].GetBlock("aab9537866677715fa3a89b157621f200a915ccdad13143976ade611f07adf40");
            var b2 = dbs[10].GetBlock("8d0dcb3c0020d33e1660ad3c0524e300314f75da4f89a9f2338e8a802070d748");

            //foreach (var db in dbs) {
            //    //Task.Run(delegate {
            //    db.PutBlock(testBlock);
            //    //}).Wait();
            //}

            //LevelDBIterator itor = new LevelDBIterator(dbs[0].DB, LevelDBReadOptions.Default);
            //itor.SeekToFirst();

            var blockHashes = new List<Bytes<Hash256>>[20];
            for (int i = 0; i < blockHashes.Length; i++) blockHashes[i] = new();

            for (int i = 0; i < addresses.Length; i++) {
                for (uint h = 1; dbs[i].GetBlock(h) is Block block; h++) {
                    //Console.WriteLine($"{h} => {block.Hash}");
                    blockHashes[i].Add(block.Hash);
                }
            }

            for (int h = 1; ; h++) {
                bool hasError = false;
                Dictionary<Bytes<Address>, Bytes<Hash256>?> dict = new();

                for (int i = 0; i < blockHashes.Length; i++) {
                    if (h - 1 < blockHashes[i].Count) {
                        dict.Add(addresses[i], blockHashes[i][h - 1]);
                    } else {
                        dict.Add(addresses[i], null);
                    }
                }

                var dist = dict.Values.Distinct().ToArray();
                if (dist.Length != 1) {
                    Console.WriteLine($"高度: {h}");
                    for (int i = 0; i < addresses.Length; i++) {
                        if (dict.TryGetValue(addresses[i], out var blockHash) && blockHash is Bytes<Hash256> hash) {
                            Console.WriteLine($"[{i,2}]{addresses[i]} => {hash}, {dbs[i].GetBlock(hash).ProducerAddress}");
                        } else {
                            Console.WriteLine($"[{i,2}]{addresses[i]} => null");
                        }
                    }
                } else if (dist[0] is null) {
                    break;
                }
            }
        }

        static void TestSignTx() {
            {
                byte[] privateKey = Config.Users[^1].PrivateKey;
                Transaction tx = new Transaction(privateKey, 2, 10, 85000, "1f03128dc8a93d94048840da95032c6c6bf407b9", 12345, Hex.ParseToBytes("ff26121ff0"));
                Console.WriteLine($"私钥: {Hex.ToString(privateKey)}");
                Console.WriteLine($"公钥: ({tx.FromPublicKey.X}, {tx.FromPublicKey.Y})");
                Console.WriteLine($"地址: {tx.From}");
                Console.WriteLine($"消息Hash: {tx.HashSignHeader}");
                Console.WriteLine($"签名: ({tx.Signature.R}, {tx.Signature.S})");
                Console.WriteLine(Hex.ToString(tx.NetworkSerialize()));

                //Transaction tx = Transaction.NetworkDeserialize(Hex.ParseToBytes("020a8898051f03128dc8a93d94048840da95032c6c6bf407b9b96005ff26121ff06a09ba39e6c29f47f23fa016adf27a9ed4fc4aeb0beb4d46a4dfd60b396e23674554f62994d705fa3d19ccfb577f5d564b35a07accf7fbee90da56d96546a84eb713e8d6c23d5067ed888657d7bf548b2dd68ebe3408fa3bb695830d17b100cd6cfa22b98a7c67e32ef9e0e1e90587a89c9925fb2c988f6f23731ed534e59741"));
                Console.WriteLine(tx.Hash);

                var tx2 = Transaction.NetworkDeserialize(tx.NetworkSerialize());
                tx2.FromPublicKey = Secp256k1.Secp256k1.RecoverPublicKey(tx2.HashSignHeader.ToArray(), tx2.Signature);
                return;
            }

            {
                byte[] privateKey = Hex.ParseToBytes("00cf284d1673d5e79c484aad6648348cbc4a653c564dde4735acf073642ab3d1");
                byte[] msg = Hex.ParseToBytes("dde8c57cd0a1093b4dd94679ced239095073f752bd9db39f820138392f720fdf");

            }

            while (true) {
                byte[] privateKey = Secp256k1.Secp256k1.CreatePrivateKey();
                PublicKey publicKey = Secp256k1.Secp256k1.CreatePublicKey(privateKey);
                Console.WriteLine($"{publicKey.X}, {publicKey.Y}");
                byte[] msg = Bytes<U256>.Random().ToArray();
                for (int i = 0; i < 100; i++) {
                    Console.WriteLine("============================================");
                    var sig = Secp256k1.Secp256k1.Sign(privateKey, msg);
                    var p = Secp256k1.Secp256k1.RecoverPublicKey(msg, sig);
                    Console.WriteLine($"{p.X}, {p.Y}");
                    //Console.Write(sig.R.v0 % 2 == 0 ? "0 " : "1 ");
                    //Console.WriteLine($"{sig.R}, {sig.S}");
                    if (p.X != publicKey.X || p.Y != publicKey.Y) {
                        throw new Exception();
                    }
                    //    Console.WriteLine($"* {p.PublicKey1.X}, {p.PublicKey1.Y}");
                    //    Console.WriteLine($"  {p.PublicKey2.X}, {p.PublicKey2.Y}");
                    //} else {
                    //    Console.WriteLine($"  {p.PublicKey1.X}, {p.PublicKey1.Y}");
                    //    Console.WriteLine($"* {p.PublicKey2.X}, {p.PublicKey2.Y}");
                    //}
                }
                break;
                //Console.WriteLine($"{publicKey2.X}, {publicKey2.Y}");
                //if (publicKey.X != publicKey2.X || publicKey.Y != publicKey2.Y) {
                //    //Console.WriteLine(Hex.ToString(privateKey));
                //    //Console.WriteLine(Hex.ToString(msg));
                //    //Console.WriteLine($"{sig.R}, {sig.S}");
                //    //Console.WriteLine(Secp256k1.Secp256k1.Verify(publicKey, msg, sig));
                //    //Console.WriteLine(Secp256k1.Secp256k1.Verify(publicKey2, msg, sig));
                //    //Console.WriteLine($"{publicKey.X}, {publicKey.Y}");
                //    //Console.WriteLine($"{publicKey2.X}, {publicKey2.Y}");
                //} else {
                //    Console.WriteLine("===================================");
                //    Console.WriteLine(Hex.ToString(privateKey));
                //    Console.WriteLine(Hex.ToString(msg));
                //    Console.WriteLine($"{sig.R}, {sig.S}");
                //    Console.WriteLine(Secp256k1.Secp256k1.Verify(publicKey, msg, sig));
                //    Console.WriteLine(Secp256k1.Secp256k1.Verify(publicKey2, msg, sig));
                //    Console.WriteLine($"{publicKey.X}, {publicKey.Y}");
                //    Console.WriteLine($"{publicKey2.X}, {publicKey2.Y}");
                //}
            }
        }

        public static readonly Dictionary<Bytes<Address>, int> clientIndexes = new Dictionary<Bytes<Address>, int>();
        public static readonly Dictionary<Bytes<Address>, string> clientNames = new Dictionary<Bytes<Address>, string>();
        public static readonly HashSet<Bytes<Address>> AllAddresses = new HashSet<Bytes<Address>>();

        static async Task Main(string[] args) {
            // Bytes<U256> v = new Bytes<U256>(0x123);
            TestSecp256k1();
            //TestSignTx();

            return;


            List<Client> clients = new List<Client>();

            const int BeginPort = 30000;
            foreach (var (privkey, _, addr) in Config.Users.Take(20).OrderBy(p => p.Address)) {
                Client c;
                int port = BeginPort + clients.Count;
                var config = new SuperNodeConfig() {
                    PrivateKey = privkey,
                };
                if (clients.Count is 0) {
                    c = new Client(addr, new IPEndPoint(IPAddress.Loopback, port), superConfig: config, name: $"client-{port}");
                } else {
                    c = new Client(addr, new IPEndPoint(IPAddress.Loopback, port), seeds: new[] { clients[0].EndPoint }, superConfig: config, name: $"client-{port}");
                }
                clientIndexes[c.Address] = clients.Count;
                clientNames[c.Address] = c.Name;
                clients.Add(c);
                Console.WriteLine($"{c.Name}: {c.Address}");
            }

            await Task.WhenAll(clients.Select(c => c.Initialization()));

            Console.ReadLine();

            List<Task> disposeTasks = new List<Task>();
            foreach (var c in clients) {
                disposeTasks.Add(c.DisposeAsync().AsTask());
                //Console.WriteLine($"Dispose: {c.Bytes<Address>}");
            }
            await Task.WhenAll(disposeTasks.ToArray());


        }
    }
}
