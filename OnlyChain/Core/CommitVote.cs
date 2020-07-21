using OnlyChain.Secp256k1;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class CommitVote {
        const byte PrecommitPrefix = 0;
        const byte CommitPrefix = 1;

        public readonly bool IsPrecommit;
        /// <summary>
        /// 上一个已确认的区块hash，用于判断进度。
        /// </summary>
        public readonly Hash<Size256> HashPrevBlock;
        /// <summary>
        /// 投给当前要确认的区块的hash。
        /// <para>如果否决该区块或未接收到区块，那么将此字段设置为0</para>
        /// </summary>
        public readonly Hash<Size256> HashVote;

        public readonly Hash<Size256> Hash;
        public readonly Signature Signature;


        public CommitVote(ReadOnlySpan<byte> rawData) {
            using Deserializer deserializer = new Deserializer(rawData);
            IsPrecommit = deserializer.Read<byte>() switch
            {
                PrecommitPrefix => true,
                CommitPrefix => false,
                _ => throw new FormatException()
            };
            HashPrevBlock = deserializer.Read<Hash<Size256>>();
            HashVote = deserializer.Read<Hash<Size256>>();
            Hash = rawData[..deserializer.Index].MessageHash();
            Signature = deserializer.Read(Deserializer.Signature);
        }

        unsafe public CommitVote(bool isPrecommit, Hash<Size256> hashPrevBlock, Hash<Size256> hashVote, ReadOnlySpan<byte> privateKey) {
            HashPrevBlock = hashPrevBlock;
            HashVote = hashVote;

            Serializer serializer = stackalloc byte[1 + sizeof(Hash<Size256>) * 2];
            serializer.Write(IsPrecommit ? PrecommitPrefix : CommitPrefix);
            serializer.Write(hashPrevBlock);
            serializer.Write(hashVote);
            Signature = serializer.Sign(privateKey, out Hash);
        }

        public static CommitVote Commit(Hash<Size256> hashPrevBlock, Hash<Size256> hashVote, ReadOnlySpan<byte> privateKey) {
            return new CommitVote(false, hashPrevBlock, hashVote, privateKey);
        }

        public static CommitVote Precommit(Hash<Size256> hashPrevBlock, Hash<Size256> hashVote, ReadOnlySpan<byte> privateKey) {
            return new CommitVote(true, hashPrevBlock, hashVote, privateKey);
        }

        unsafe public byte[] Serialize() {
            Serializer serializer = stackalloc byte[1 + sizeof(Hash<Size256>) * 2 + 64];
            serializer.Write(IsPrecommit ? PrecommitPrefix : CommitPrefix);
            serializer.Write(HashPrevBlock);
            serializer.Write(HashVote);
            serializer.Write(Serializer.Signature, Signature);
            return serializer.RawData.ToArray();
        }
    }
}
