#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System.Buffers.Binary;

namespace OnlyChain.Core {
    unsafe public ref struct Serializer {
        public delegate int Writer<T>(ref Serializer s, T value);

        public byte[]? Data;
        public int Index;

        public readonly ReadOnlySpan<byte> RawData => Data.AsSpan(0, Index);

        public Serializer(int capacity) {
            Data = new byte[capacity];
            Index = 0;
        }

        public void Write<T>(in T value) where T : unmanaged {
            Reserved(sizeof(T));
            Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(WriteSpan)) = value;
            Index += sizeof(T);
        }

        public void Write(ReadOnlySpan<byte> value) {
            Reserved(value.Length);
            value.CopyTo(WriteSpan);
            Index += value.Length;
        }

        public void Reserved(int size) {
            if (Data is null) {
                Array.Resize(ref Data, Math.Max(size, 1024));
            } else if (Data.Length < Index + size) {
                int newSize = Math.Max(Data.Length * 3 / 2, Index + size);
                Array.Resize(ref Data, newSize);
            }
        }


        private readonly Span<byte> WriteSpan => Data.AsSpan(Index);


        public void WritePublicKey(PublicKey value) {
            Reserved(33);
            Index += value.Serialize(WriteSpan, compressed: true);
        }

        public void WritePublicKeyStruct(PublicKeyStruct value) {
            Reserved(sizeof(Secp256k1.Math.U256) * 2);
            value.X.CopyTo(WriteSpan[..sizeof(Secp256k1.Math.U256)], bigEndian: true);
            value.Y.CopyTo(WriteSpan[sizeof(Secp256k1.Math.U256)..], bigEndian: true);
            Index += sizeof(Secp256k1.Math.U256) * 2;
        }

        public void WriteSignature(Signature value) {
            Reserved(sizeof(Secp256k1.Math.U256) * 2);
            value.GetR(WriteSpan[..sizeof(Secp256k1.Math.U256)]);
            value.GetS(WriteSpan[sizeof(Secp256k1.Math.U256)..]);
            Index += sizeof(Secp256k1.Math.U256) * 2;
        }

        public void WriteVarUInt(ulong value) {
            Reserved(9);
            Index += WriteSpan.WriteVarUInt(value);
        }

        public void WriteTxData(byte[] value) {
            if (value.Length > 524288) throw new InvalidOperationException();
            Reserved(9 + value.Length);
            int length = WriteSpan.WriteVarUInt((ulong)value.Length);
            value.CopyTo(WriteSpan[length..]);
            Index += length + value.Length;
        }

        public void WriteU64LittleEndian(ulong value) {
            Reserved(sizeof(ulong));
            BinaryPrimitives.WriteUInt64LittleEndian(WriteSpan, value);
            Index += sizeof(ulong);
        }

        public void WriteAddresses(ReadOnlySpan<Bytes<Address>> value) {
            if (value.Length >= 256) throw new InvalidOperationException();
            Reserved(1 + value.Length * sizeof(Address));
            WriteSpan[0] = (byte)value.Length;
            MemoryMarshal.Cast<Bytes<Address>, byte>(value).CopyTo(WriteSpan[1..]);
            Index += 1 + value.Length * sizeof(Address);
        }

        public void WriteBool(bool value) {
            Reserved(1);
            WriteSpan[0] = (byte)(value ? 0 : 1);
            Index += 1;
        }
    }
}
