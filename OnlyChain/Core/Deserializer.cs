using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace OnlyChain.Core {
    unsafe public ref struct Deserializer {
        public delegate (T Value, int Length) Reader<T>(ReadOnlySpan<byte> data);

        public readonly ReadOnlySpan<byte> Data;
        public int Index;

        public Deserializer(ReadOnlySpan<byte> data) {
            Data = data;
            Index = 0;
        }

        public T Read<T>(Reader<T> reader) {
            var (value, length) = reader(Data[Index..]);
            Index += length;
            return value;
        }

        public ref readonly T Read<T>() where T : unmanaged {
            ref var value = ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(Data), Index));
            Index += sizeof(T);
            return ref value;
        }

        public T[] ReadValues<T>(Reader<T> reader, int count) {
            var result = new T[count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = Read(reader);
            }
            return result;
        }

        public T[] ReadValues<T>(int count) where T : unmanaged {
            var result = new T[count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = Read<T>();
            }
            return result;
        }

        public void Dispose() {
            if (Index != Data.Length) throw new FormatException();
        }


        public readonly static Reader<PublicKey> PublicKey = data => (Secp256k1.PublicKey.Parse(data, out int length), length);

        public readonly static Reader<PublicKey> PublicKeyStruct = data => {
            U256 x = new U256(data[..sizeof(U256)], bigEndian: true);
            U256 y = new U256(data[sizeof(U256)..(sizeof(U256) * 2)], bigEndian: true);
            return (new PublicKey(x, y), sizeof(U256) * 2);
        };

        public readonly static Reader<Signature> Signature = data => {
            U256 r = new U256(data[..sizeof(U256)], bigEndian: true);
            U256 s = new U256(data[sizeof(U256)..(sizeof(U256) * 2)], bigEndian: true);
            return (new Signature(r, s), sizeof(U256) * 2);
        };

        public readonly static Reader<ulong> VarUInt = data => (data.ReadVarUInt(out int length), length);

        public readonly static Reader<uint> VarUInt32 = data => {
            ulong v = data.ReadVarUInt(out int length);
            if (v > uint.MaxValue) throw new FormatException();
            return ((uint)v, length);
        };

        public readonly static Reader<ushort> VarUInt16 = data => {
            ulong v = data.ReadVarUInt(out int length);
            if (v > ushort.MaxValue) throw new FormatException();
            return ((ushort)v, length);
        };

        public readonly static Reader<byte[]> TxData = data => {
            ulong bytes = data.ReadVarUInt(out int length);
            if (bytes > 524288) throw new FormatException();
            if (bytes is 0) return (Array.Empty<byte>(), length);
            return (data.Slice(length, (int)bytes).ToArray(), length + (int)bytes);
        };

        public readonly static Reader<bool> Bool = data => {
            return (data[0] != 0, 1);
        };
    }
}
