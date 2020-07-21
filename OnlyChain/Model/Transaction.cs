#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using OnlyChain.Core;
using OnlyChain.Secp256k1;

namespace OnlyChain.Model {
    public sealed class Transaction {
        public readonly ulong Nonce;
        public readonly Coin GasPrice;
        public readonly ulong GasLimit;
        public readonly Address To;
        public readonly Coin Value;
        public readonly byte[] Data;
        public readonly PublicKey PublicKey;
        public readonly Signature Signature;

        public readonly Hash<Size256> HashSignHeader;
        public readonly Hash<Size256> Hash;
        public readonly Address From;
        /// <summary>
        /// 最低要消耗的汽油费
        /// </summary>
        public ulong BaseGasUsed { get; internal set; }
        /// <summary>
        /// 合约消耗的汽油费
        /// </summary>
        public ulong ContractGasUsed { get; internal set; }
        public int BlockHeight { get; internal set; }
        public AttachData? AttachData { get; internal set; }
        /// <summary>
        /// 网络序列化后的字节数
        /// </summary>
        public int Bytes { get; }

        /// <summary>
        /// 从网络反序列化交易
        /// </summary>
        /// <remarks>
        /// varint: Nonce, 严格递增的交易编号
        /// varint: GasPrice, 每汽油费用
        /// varint: GasLimit, 愿意支付的最大汽油量
        /// 20 bytes: To, 收款人
        /// varint: Value, 转账代币数
        /// varint: Data长度
        /// 0-n bytes: Data
        /// 64 bytes: 付款人公钥
        /// 64 bytes: 签名
        /// </remarks>
        /// <param name="rawData"></param>
        Transaction(ReadOnlySpan<byte> rawData) {
            var deserializer = new Deserializer(rawData);
            Nonce = deserializer.Read(Deserializer.VarUInt);
            GasPrice = deserializer.Read(Deserializer.VarUInt);
            GasLimit = deserializer.Read(Deserializer.VarUInt);
            To = deserializer.Read<Address>();
            Value = deserializer.Read(Deserializer.VarUInt);
            Data = deserializer.Read(Deserializer.TxData);
            PublicKey = deserializer.Read(Deserializer.PublicKeyStruct);
            HashSignHeader = rawData[..deserializer.Index].MessageHash();
            Signature = deserializer.Read(Deserializer.Signature);

            From = PublicKey.ToAddress();
            Hash = rawData[..deserializer.Index].MessageHash();
            Bytes = deserializer.Index;

            BaseGasUsed = (ulong)(21000 + Data.Length * 200);

            AttachData = AttachData.ParseData(this);
        }

        Transaction(ReadOnlySpan<byte> rawData, Hash<Size256> txHash, UserDictionary userDictionary) {
            Hash = txHash;

            var deserializer = new Deserializer(rawData);
            BlockHeight = (int)deserializer.Read(Deserializer.VarUInt);
            Nonce = deserializer.Read(Deserializer.VarUInt);
            GasPrice = deserializer.Read(Deserializer.VarUInt);
            GasLimit = deserializer.Read(Deserializer.VarUInt);
            ulong toIndex = deserializer.Read(Deserializer.VarUInt);
            To = userDictionary.GetAddress((int)toIndex);
            Value = deserializer.Read(Deserializer.VarUInt);
            Data = deserializer.Read(Deserializer.TxData);
            ulong fromIndex = deserializer.Read(Deserializer.VarUInt);
            From = userDictionary.GetAddress((int)fromIndex);
            PublicKey = userDictionary.GetPublicKey((int)fromIndex);
            Signature = deserializer.Read(Deserializer.Signature);

            BaseGasUsed = (ulong)(21000 + Data.Length * 200);

            AttachData = AttachData.ParseData(this);
        }

        unsafe public Transaction(ReadOnlySpan<byte> privateKey, ulong nonce, Coin gasPrice, ulong gasLimit, Address to, Coin value, byte[]? data = null) {
            PublicKey = Secp256k1.Secp256k1.CreatePublicKey(privateKey);
            From = PublicKey.ToAddress();
            Nonce = nonce;
            GasPrice = gasPrice;
            GasLimit = gasLimit;
            To = to;
            Value = value;
            Data = data ?? Array.Empty<byte>();

            Span<byte> buffer = stackalloc byte[NetworkMaxBufferLength(Data.Length)];
            var serializer = new Serializer(buffer);
            NetworkSerializeWithoutSignature(ref serializer);
            Hash<Size256> messageHash = HashTools.MessageHash(buffer[..serializer.Index]);
            Signature = Ecdsa.Sign(privateKey, messageHash);
            HashSignHeader = messageHash;

            serializer.Write(Serializer.Signature, Signature);
            Hash = HashTools.MessageHash(buffer[..serializer.Index]);

            AttachData = AttachData.ParseData(this);
        }

        public byte[] NetworkSerialize() {
            Span<byte> buffer = stackalloc byte[NetworkMaxBufferLength(Data.Length)];
            int length = NetworkSerialize(buffer);
            return buffer[..length].ToArray();
        }

        public int NetworkSerialize(Span<byte> buffer) {
            var serializer = new Serializer(buffer);
            NetworkSerializeWithoutSignature(ref serializer);
            serializer.Write(Serializer.Signature, Signature);
            return serializer.Index;
        }

        private void NetworkSerializeWithoutSignature(ref Serializer serializer) {
            serializer.Write(Serializer.VarUInt, Nonce);
            serializer.Write(Serializer.VarUInt, GasPrice);
            serializer.Write(Serializer.VarUInt, GasLimit);
            serializer.Write(To);
            serializer.Write(Serializer.VarUInt, Value);
            serializer.Write(Serializer.TxData, Data);
            serializer.Write(Serializer.PublicKeyStruct, PublicKey);
        }

        unsafe public static int NetworkMaxBufferLength(int dataBytes) {
            return 9 * 3 + 20 + 9 + 9 + dataBytes + 64 + 64;
        }


        public byte[] NativeSerialize(uint blockHeight, UserDictionary userDictionary) {
            Span<byte> buffer = stackalloc byte[NativekMaxBufferLength(Data.Length)];
            int length = NativeSerialize(buffer, blockHeight, userDictionary);
            return buffer[..length].ToArray();
        }

        public int NativeSerialize(Span<byte> buffer, uint nativeBlockIndex, UserDictionary userDictionary) {
            var serializer = new Serializer(buffer);
            serializer.Write(Serializer.VarUInt, nativeBlockIndex);
            serializer.Write(Serializer.VarUInt, Nonce);
            serializer.Write(Serializer.VarUInt, GasPrice);
            serializer.Write(Serializer.VarUInt, GasLimit);
            int toIndex = userDictionary.GetOrCreateIndex(To);
            serializer.Write(Serializer.VarUInt, (ulong)toIndex);
            serializer.Write(Serializer.VarUInt, Value);
            serializer.Write(Serializer.TxData, Data);
            int fromIndex = userDictionary.Set(From, PublicKey);
            serializer.Write(Serializer.VarUInt, (ulong)fromIndex);
            serializer.Write(Serializer.Signature, Signature);
            return serializer.Index;
        }

        unsafe public static int NativekMaxBufferLength(int dataBytes) {
            return 9 * 7 + dataBytes + 9 + 64;
        }

        /// <summary>
        /// 反序列化来自网络的交易数据
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static Transaction NetworkDeserialize(ReadOnlySpan<byte> rawData)
            => new Transaction(rawData);

        /// <summary>
        /// 反序列化来自本地的交易数据
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static Transaction NativeDeserialize(ReadOnlySpan<byte> rawData, Hash<Size256> txHash, UserDictionary userDictionary)
            => new Transaction(rawData, txHash, userDictionary);

        public override bool Equals(object? obj) {
            return obj is Transaction other && (ReferenceEquals(this, other) || Hash == other.Hash);
        }

        public override int GetHashCode() => Hash.GetHashCode();

        public override string ToString() {
            return AttachData switch
            {
                null => $"普通交易: {Hash}",
                SuperPledgeData _ => $"超级节点质押: {Hash}",
                SuperRedemptionData _ => $"超级节点赎回: {Hash}",
                VoteData _ => $"投票: {Hash}",
                LockData _ => $"锁仓交易: {Hash}",
                _ => $"未知交易: {Hash}"
            };
        }
    }
}
