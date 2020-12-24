#nullable enable

using OnlyChain.Model;
using OnlyChain.Secp256k1;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OnlyChain.Core {
    public sealed class Block : IComparable<Block> {
        public uint Version;
        public uint Height;
        /// <summary>
        /// 从0开始，若没有发生跳过出块人的情况，则每次自增2。
        /// </summary>
        public uint Timestamp;
        public Bytes<Hash256> HashPrevBlock;
        public Bytes<Hash256> HashWorldState;
        public Bytes<Hash256> HashTxMerkleRoot;
        public Bytes<Address> ProducerAddress;
        public Signature Signature;

        public Bytes<Hash256> HashSignHeader;
        public Bytes<Hash256> Hash;
        public PublicKey ProducerPublicKey = null!;
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



#pragma warning disable CS8618 // 不可为 null 的字段未初始化。请考虑声明为可以为 null。
        public Block() {
        }
#pragma warning restore CS8618 // 不可为 null 的字段未初始化。请考虑声明为可以为 null。

        /// <summary>
        /// 解析来自网络的区块（需要外部验证）
        /// </summary>
        /// <param name="rawData"></param>
        unsafe public Block(ReadOnlySpan<byte> rawData, bool hasTx = true) {
            var deserializer = new Deserializer(rawData);
            Version = deserializer.Read(Deserializer.VarUInt32);
            Height = deserializer.Read(Deserializer.VarUInt32);
            Timestamp = deserializer.Read(Deserializer.VarUInt32);
            HashPrevBlock = deserializer.Read<Bytes<Hash256>>();
            HashWorldState = deserializer.Read<Bytes<Hash256>>();
            HashTxMerkleRoot = deserializer.Read<Bytes<Hash256>>();
            ProducerAddress = deserializer.Read<Bytes<Address>>();
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

        //unsafe public Block(uint height, uint timestamp, Bytes<Hash256> hashPrevBlock, ReadOnlySpan<byte> producerPrivateKey, Transaction[] transactions, BlockState commitStatus) {
        //    Version = Constants.BlockVersion;
        //    Height = height;
        //    Timestamp = timestamp;
        //    HashPrevBlock = hashPrevBlock;
        //    HashWorldState = commitStatus.WorldState.RootHash;
        //    HashTxMerkleRoot = commitStatus.Transactions.RootHash;
        //    CommitState = commitStatus;
        //    Transactions = transactions;

        //    Serializer serializer = new Serializer();
        //    NetworkSerializeHeaderWithoutSignature(ref serializer);
        //    Bytes<Hash256> signHash = serializer.RawData.MessageHash();
        //    Signature = Ecdsa.Sign(producerPrivateKey, signHash);
        //    serializer.WriteSignature(Signature);
        //    Hash = serializer.RawData.MessageHash();
        //}

        public void NetworkSerializeHeaderWithoutSignature(ref Serializer serializer) {
            serializer.WriteVarUInt(Version);
            serializer.WriteVarUInt(Height);
            serializer.WriteVarUInt(Timestamp);
            serializer.Write(HashPrevBlock);
            serializer.Write(HashWorldState);
            serializer.Write(HashTxMerkleRoot);
            serializer.Write(ProducerAddress);
        }


        unsafe public byte[] NetworkSerialize() {
            Serializer serializer = new Serializer();
            SerializeHeader(ref serializer);

            serializer.WriteVarUInt((ulong)Transactions.Length);
            foreach (var tx in Transactions) {
                tx.NetworkSerialize(ref serializer);
            }
            return serializer.RawData.ToArray();
        }

        public void ComputeHash() {
            Serializer serializer = new Serializer();
            SerializeHeader(ref serializer);
            Hash = serializer.RawData.MessageHash();
        }

        public void SerializeHeader(ref Serializer serializer) {
            NetworkSerializeHeaderWithoutSignature(ref serializer);
            serializer.WriteSignature(Signature);
        }

        public byte[] SerializeHeader() {
            Serializer serializer = new Serializer();
            SerializeHeader(ref serializer);
            return serializer.RawData.ToArray();
        }

        public void SignComputeHash(ReadOnlySpan<byte> privateKey) {
            Serializer serializer = new Serializer();
            NetworkSerializeHeaderWithoutSignature(ref serializer);
            HashSignHeader = serializer.RawData.MessageHash();
            Signature = Ecdsa.Sign(privateKey, HashSignHeader);
            serializer.WriteSignature(Signature);
            Hash = serializer.RawData.MessageHash();
        }

        public override bool Equals(object? obj) => obj is Block other && Hash == other.Hash;

        public override int GetHashCode() => Hash.GetHashCode();

        public int CompareTo(Block? other) {
            Debug.Assert(other is not null);
            return Height.CompareTo(other.Height);
        }
    }
}
