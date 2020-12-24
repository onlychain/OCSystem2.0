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
        public readonly Bytes<Hash256> HashPrevBlock;
        /// <summary>
        /// 投给当前要确认的区块的hash。
        /// <para>如果否决该区块或未接收到区块，那么将此字段设置为0</para>
        /// </summary>
        public readonly Bytes<Hash256> HashVote;

        public readonly Bytes<Hash256> Hash;
        public readonly Signature Signature;


        public CommitVote(ReadOnlySpan<byte> rawData) {
            using Deserializer deserializer = new Deserializer(rawData);
            IsPrecommit = deserializer.Read<byte>() switch
            {
                PrecommitPrefix => true,
                CommitPrefix => false,
                _ => throw new FormatException()
            };
            HashPrevBlock = deserializer.Read<Bytes<Hash256>>();
            HashVote = deserializer.Read<Bytes<Hash256>>();
            Hash = rawData[..deserializer.Index].MessageHash();
            Signature = deserializer.Read(Deserializer.Signature);
        }

        unsafe public CommitVote(bool isPrecommit, Bytes<Hash256> hashPrevBlock, Bytes<Hash256> hashVote, ReadOnlySpan<byte> privateKey) {
            IsPrecommit = isPrecommit;
            HashPrevBlock = hashPrevBlock;
            HashVote = hashVote;

            Serializer serializer = new Serializer();
            serializer.Write(IsPrecommit ? PrecommitPrefix : CommitPrefix);
            serializer.Write(hashPrevBlock);
            serializer.Write(hashVote);
            Signature = serializer.Sign(privateKey, out Hash);
        }

        public static CommitVote Commit(Bytes<Hash256> hashPrevBlock, Bytes<Hash256> hashVote, ReadOnlySpan<byte> privateKey) {
            return new CommitVote(false, hashPrevBlock, hashVote, privateKey);
        }

        public static CommitVote Precommit(Bytes<Hash256> hashPrevBlock, Bytes<Hash256> hashVote, ReadOnlySpan<byte> privateKey) {
            return new CommitVote(true, hashPrevBlock, hashVote, privateKey);
        }

        unsafe public byte[] Serialize() {
            Serializer serializer = new Serializer();
            serializer.Write(IsPrecommit ? PrecommitPrefix : CommitPrefix);
            serializer.Write(HashPrevBlock);
            serializer.Write(HashVote);
            serializer.WriteSignature(Signature);
            return serializer.RawData.ToArray();
        }
    }
}
