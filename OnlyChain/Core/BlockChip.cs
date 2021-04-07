using OnlyChain.Coding;
using OnlyChain.Secp256k1;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class BlockChip {
        public readonly Bytes<Hash160> Id;
        public readonly int TotalCount;
        public readonly int RestoreCount;
        public readonly int Index;
        public readonly int BlockBytes;
        public readonly byte[] Data;
        public readonly Signature Signature;

        private byte[] rawDataWithoutSignature;
        public Bytes<Hash256> MessageHash { get; private set; }
        public PublicKey PublicKey;

        // 20 bytes: 碎片id
        // 1 byte: 碎片总数
        // 1 byte: 需要集齐的碎片数
        // 1 byte: 碎片序号
        // VarInt: 原始数据字节数
        // n bytes: 碎片数据
        // 64 bytes: 签名
        private BlockChip(ReadOnlySpan<byte> rawData) {
            Deserializer deserializer = new Deserializer(rawData);
            Id = deserializer.Read<Bytes<Hash160>>();
            TotalCount = deserializer.Read<byte>();
            RestoreCount = deserializer.Read<byte>();
            Index = deserializer.Read<byte>();
            uint blockBytes = deserializer.Read(Deserializer.VarUInt32);
            if (blockBytes > Constants.MaxBlockBytes) throw new InvalidBlockChipException();
            BlockBytes = (int)blockBytes;
            int chipBytes = (BlockBytes + RestoreCount - 1) / RestoreCount;
            Data = rawData.Slice(deserializer.Index, chipBytes).ToArray();
            deserializer.Index += chipBytes;
            MessageHash = rawData[..deserializer.Index].MessageHash();
            Signature = deserializer.Read(Deserializer.Signature);
            if (deserializer.Index != rawData.Length) {
                throw new InvalidBlockChipException();
            }

            PublicKey = Ecdsa.RecoverPublicKey(MessageHash, Signature);
        }

        public BlockChip(Bytes<Hash160> id, int totalCount, int restoreCount, int index, int blockBytes, byte[] data, ReadOnlySpan<byte> privateKey) {
            Id = id;
            TotalCount = totalCount;
            RestoreCount = restoreCount;
            Index = index;
            BlockBytes = blockBytes;
            Data = data;

            rawDataWithoutSignature = SerializeWithoutSignature();
            MessageHash = HashTools.MessageHash(rawDataWithoutSignature);
            Signature = Ecdsa.Sign(privateKey, MessageHash);
        }

        unsafe private byte[] SerializeWithoutSignature() {
            var serializer = new Serializer();
            serializer.Write(Id);
            serializer.Write((byte)TotalCount);
            serializer.Write((byte)RestoreCount);
            serializer.Write((byte)Index);
            serializer.WriteVarUInt((ulong)BlockBytes);
            serializer.Write(Data);
            return serializer.RawData.ToArray();
        }

        public byte[] Serialize() {
            if (rawDataWithoutSignature is null) {
                rawDataWithoutSignature = SerializeWithoutSignature();
            }

            var serializer = new Serializer();
            serializer.Write(rawDataWithoutSignature);
            serializer.WriteSignature(Signature);
            return serializer.RawData.ToArray();
        }

        unsafe public bool Verify(PublicKey publicKey) {
            return Ecdsa.Verify(publicKey, MessageHash, Signature);
        }



        public static BlockChip Parse(ReadOnlySpan<byte> rawData) {
            return new BlockChip(rawData);
        }

        unsafe public static BlockChip[] Split(Block block, int totalCount, int restoreCount, ReadOnlySpan<byte> privateKey) {
            var data = block.NetworkSerialize();
            var paddingData = data;
            int ecCount = totalCount - restoreCount;
            if (data.Length % restoreCount != 0) {
                paddingData = new byte[data.Length + (restoreCount - data.Length % restoreCount)];
                data.AsSpan().CopyTo(paddingData);
            }
            int rows = paddingData.Length / restoreCount;
            var ec = new byte[ecCount * rows];
            if (ecCount > 0) {
                ErasureCoding.Encode(paddingData, ec, restoreCount, ecCount);
            }

            var chipDatas = new byte[totalCount][];
            fixed (byte* @in = paddingData) {
                for (int i = 0; i < restoreCount; i++) {
                    chipDatas[i] = new byte[rows];

                    int pointer = i;
                    fixed (byte* @out = chipDatas[i]) {
                        for (int j = 0; j < rows; j++, pointer += restoreCount) {
                            @out[j] = @in[pointer];
                        }
                    }
                }
            }
            fixed (byte* @in = ec) {
                for (int i = 0; i < ecCount; i++) {
                    chipDatas[restoreCount + i] = new byte[rows];

                    int pointer = i;
                    fixed (byte* @out = chipDatas[restoreCount + i]) {
                        for (int j = 0; j < rows; j++, pointer += ecCount) {
                            @out[j] = @in[pointer];
                        }
                    }
                }
            }

            Bytes<Hash160> id = MakeId(block.Hash, totalCount, restoreCount);
            var chips = new BlockChip[totalCount];
            for (int i = 0; i < chips.Length; i++) {
                chips[i] = new BlockChip(id, totalCount, restoreCount, i, data.Length, chipDatas[i], privateKey);
            }
            return chips;
        }

        public static Bytes<Hash160> MakeId(Bytes<Hash256> blockHash, int totalCount, int restoreCount) {
            Serializer serializer = new Serializer();
            serializer.Write(blockHash);
            serializer.Write((byte)totalCount);
            serializer.Write((byte)restoreCount);
            return serializer.RawData.MessageHash160();
        }
    }
}
