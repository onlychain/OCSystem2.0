#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System.Numerics;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace OnlyChain.Core {
    unsafe public ref struct Serializer {
        public delegate int Writer<T>(Span<byte> data, T value);

        public readonly Span<byte> Data;
        public int Index;

        public readonly ReadOnlySpan<byte> RawData => Data[..Index];

        public Serializer(Span<byte> data) {
            Data = data;
            Index = 0;
        }

        public void Write<T>(Writer<T> writer, T value) {
            Index += writer(Data[Index..], value);
        }

        public void Write<T>(in T value) where T : unmanaged {
            Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(Data), Index)) = value;
            Index += sizeof(T);
        }

        public void Write(ReadOnlySpan<byte> value) {
            value.CopyTo(Data[Index..]);
            Index += value.Length;
        }

        public static implicit operator Serializer(Span<byte> buffer) => new Serializer(buffer);



        public readonly static Writer<PublicKey> PublicKey = (data, value) => value.Serialize(data, compressed: true);

        public readonly static Writer<PublicKey> PublicKeyStruct = (data, value) => {
            value.GetX(data[0..sizeof(U256)]);
            value.GetY(data[sizeof(U256)..(sizeof(U256) * 2)]);
            return sizeof(U256) * 2;
        };

        public readonly static Writer<Signature> Signature = (data, value) => {
            value.GetR(data[0..sizeof(U256)]);
            value.GetS(data[sizeof(U256)..(sizeof(U256) * 2)]);
            return sizeof(U256) * 2;
        };

        public readonly static Writer<ulong> VarUInt = (data, value) => data.WriteVarUInt(value);

        public readonly static Writer<byte[]> TxData = (data, value) => {
            if (value.Length > 524288) throw new InvalidOperationException();
            int length = data.WriteVarUInt((ulong)value.Length);
            value.CopyTo(data[length..]);
            return length + value.Length;
        };

        public readonly static Writer<ulong> U64LittleEndian = (data, value) => {
            if (!BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            BinaryPrimitives.WriteUInt64LittleEndian(data, value);
            return sizeof(ulong);
        };

        public readonly static Writer<Address[]?> Addresses = (data, value) => {
            value ??= Array.Empty<Address>();
            data[0] = (byte)value.Length;
            MemoryMarshal.Cast<Address, byte>(value).CopyTo(data[1..]);
            return 1 + value.Length * sizeof(Address);
        };

        public readonly static Writer<bool> Bool = (data, value) => {
            data[0] = (byte)(value ? 0 : 1);
            return 1;
        };
    }
}
