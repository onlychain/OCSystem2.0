#nullable enable

using OnlyChain.Model;
using OnlyChain.Secp256k1;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace OnlyChain.Core {
    public sealed class Block {
        public uint Version;
        public uint Height;
        /// <summary>
        /// 从0开始，若没有发生跳过出块人的情况，则每次自增2。
        /// </summary>
        public uint Timestamp;
        public Hash<Size256> HashPrevBlock;
        public Hash<Size256> HashWorldState;
        public Hash<Size256> HashTxMerkleRoot;
        public Signature Signature;

        public Hash<Size256> HashSignHeader;
        public Hash<Size256> Hash;
        public Transaction[] Transactions;

        /// <remarks>
        /// <para>检查执行区块前，将此字段设置成上一个区块的<see cref="CommitState"/>的副本。</para>
        /// <para>每执行一个交易，此字段将发生变化。</para>
        /// </remarks>
        public BlockState? PrecommitState { get; set; }

        /// <summary>
        /// <para>区块最终确认的状态。</para>
        /// </summary>
        public BlockState? CommitState { get; set; }

        /// <summary>
        /// 时间戳对应的UTC时间。
        /// </summary>
        public DateTime DateTime => BlockChainTimestamp.ToDateTime(Timestamp);

        public Round SmallRound {
            get {
                if (Height == 1) return Round.InvalidValue;
                long rounds = Math.DivRem(Timestamp - 2, Constants.SmallRoundSeconds, out long index) + 1;
                return new Round(checked((int)rounds), (int)index / 2);
            }
        }

        public Round BigRound {
            get {
                if (Height == 1) return Round.InvalidValue;
                long rounds = Math.DivRem(Timestamp - 2, Constants.BigRoundSeconds, out long index) + 1;
                return new Round(checked((int)rounds), (int)index / 2);
            }
        }

        public Address ProducerAddress {
            get {
                if (Height == 1) throw new InvalidOperationException();
                BlockState state = PrecommitState ?? CommitState ?? throw new InvalidOperationException();
                return state.SortedCampaignNodes[SmallRound.IndexInRound];
            }
        }

        public Block() {
            Signature = null!;
            Transactions = null!;
        }

        /// <summary>
        /// 解析来自网络的区块（需要外部验证）
        /// </summary>
        /// <param name="rawData"></param>
        unsafe public Block(ReadOnlySpan<byte> rawData, bool hasTx = true) {
            var deserializer = new Deserializer(rawData);
            Version = deserializer.Read(Deserializer.VarUInt32);
            Height = deserializer.Read(Deserializer.VarUInt32);
            Timestamp = deserializer.Read(Deserializer.VarUInt32);
            HashPrevBlock = deserializer.Read<Hash<Size256>>();
            HashWorldState = deserializer.Read<Hash<Size256>>();
            HashTxMerkleRoot = deserializer.Read<Hash<Size256>>();
            HashSignHeader = rawData[..deserializer.Index].MessageHash();
            Signature = deserializer.Read(Deserializer.Signature);

            Hash = rawData[..deserializer.Index].MessageHash();

            if (hasTx) {
                int count = deserializer.Read(Deserializer.VarUInt16);
                var transactions = new Transaction[count];
                int offset = deserializer.Index;
                for (int i = 0; i < transactions.Length; i++) {
                    transactions[i] = Transaction.NetworkDeserialize(rawData[offset..]);
                    offset += transactions[i].Bytes;
                }
                Transactions = transactions;
            } else {
                Transactions = Array.Empty<Transaction>();
            }
        }

        unsafe public Block(uint height, uint timestamp, Hash<Size256> hashPrevBlock, ReadOnlySpan<byte> producerPrivateKey, Transaction[] transactions, BlockState commitStatus) {
            Version = Constants.BlockVersion;
            Height = height;
            Timestamp = timestamp;
            HashPrevBlock = hashPrevBlock;
            HashWorldState = commitStatus.WorldState.RootHash;
            HashTxMerkleRoot = commitStatus.Transactions.RootHash;
            CommitState = commitStatus;
            Transactions = transactions;

            Serializer serializer = stackalloc byte[NetworkMaxHeaderBytes()];
            NetworkSerializeHeaderWithoutSignature(ref serializer);
            Hash<Size256> signHash = serializer.RawData.MessageHash();
            Signature = Ecdsa.Sign(producerPrivateKey, signHash);
            serializer.Write(Serializer.Signature, Signature);
            Hash = serializer.RawData.MessageHash();
        }

        public void NetworkSerializeHeaderWithoutSignature(ref Serializer serializer) {
            serializer.Write(Serializer.VarUInt, Version);
            serializer.Write(Serializer.VarUInt, Height);
            serializer.Write(Serializer.VarUInt, Timestamp);
            serializer.Write(HashPrevBlock);
            serializer.Write(HashWorldState);
            serializer.Write(HashTxMerkleRoot);
        }

        public static int NetworkMaxHeaderBytes() {
            return 9 * 3 + 32 * 3 + 33 + 64;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static Block GenesisBlock => throw new NotImplementedException();



        unsafe public byte[] NetworkSerialize() {
            Serializer serializer = stackalloc byte[NetworkMaxHeaderBytes()];
            NetworkSerializeHeaderWithoutSignature(ref serializer);
            serializer.Write(Serializer.Signature, Signature);

            using var mem = new MemoryStream();
            mem.Write(serializer.RawData);
            mem.WriteVarUInt((ulong)Transactions.Length);
            foreach (var tx in Transactions) {
                mem.Write(tx.NetworkSerialize());
            }
            return mem.ToArray();
        }
    }
}
