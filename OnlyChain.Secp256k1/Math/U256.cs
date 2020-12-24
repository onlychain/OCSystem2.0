using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using static OnlyChain.Secp256k1.Math.Native;

namespace OnlyChain.Secp256k1.Math {
    /// <summary>
    /// 256位无符号整型
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    unsafe public readonly struct U256 {
        public readonly ulong v0, v1, v2, v3;

        public static readonly U256 Zero = new U256();
        public static readonly U256 One = new U256(1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256(ReadOnlySpan<byte> value, bool bigEndian = false) {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length != 32) throw new ArgumentException("必须是32字节", nameof(value));
            if (!bigEndian) {
                v0 = BinaryPrimitives.ReadUInt64LittleEndian(value);
                v1 = BinaryPrimitives.ReadUInt64LittleEndian(value.Slice(8));
                v2 = BinaryPrimitives.ReadUInt64LittleEndian(value.Slice(16));
                v3 = BinaryPrimitives.ReadUInt64LittleEndian(value.Slice(24));
            } else {
                v0 = BinaryPrimitives.ReadUInt64BigEndian(value.Slice(24));
                v1 = BinaryPrimitives.ReadUInt64BigEndian(value.Slice(16));
                v2 = BinaryPrimitives.ReadUInt64BigEndian(value.Slice(8));
                v3 = BinaryPrimitives.ReadUInt64BigEndian(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256(ulong value) {
            v0 = value;
            v1 = 0;
            v2 = 0;
            v3 = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256(ulong v0, ulong v1, ulong v2, ulong v3) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:使用范围运算符", Justification = "<挂起>")]
        public U256(ReadOnlySpan<char> hexString) {
            switch ((hexString.Length + 15) >> 4) {
                case 0:
                    v0 = 0;
                    v1 = 0;
                    v2 = 0;
                    v3 = 0;
                    break;
                case 1:
                    v0 = ulong.Parse(hexString, NumberStyles.HexNumber);
                    v1 = 0;
                    v2 = 0;
                    v3 = 0;
                    break;
                case 2:
                    v0 = ulong.Parse(hexString.Slice(hexString.Length - 16, 16), NumberStyles.HexNumber);
                    v1 = ulong.Parse(hexString.Slice(0, hexString.Length - 16), NumberStyles.HexNumber);
                    v2 = 0;
                    v3 = 0;
                    break;
                case 3:
                    v0 = ulong.Parse(hexString.Slice(hexString.Length - 16, 16), NumberStyles.HexNumber);
                    v1 = ulong.Parse(hexString.Slice(hexString.Length - 32, 16), NumberStyles.HexNumber);
                    v2 = ulong.Parse(hexString.Slice(0, hexString.Length - 32), NumberStyles.HexNumber);
                    v3 = 0;
                    break;
                case 4:
                    v0 = ulong.Parse(hexString.Slice(hexString.Length - 16, 16), NumberStyles.HexNumber);
                    v1 = ulong.Parse(hexString.Slice(hexString.Length - 32, 16), NumberStyles.HexNumber);
                    v2 = ulong.Parse(hexString.Slice(hexString.Length - 48, 16), NumberStyles.HexNumber);
                    v3 = ulong.Parse(hexString.Slice(0, hexString.Length - 48), NumberStyles.HexNumber);
                    break;
                default:
                    v0 = ulong.Parse(hexString.Slice(hexString.Length - 16, 16), NumberStyles.HexNumber);
                    v1 = ulong.Parse(hexString.Slice(hexString.Length - 32, 16), NumberStyles.HexNumber);
                    v2 = ulong.Parse(hexString.Slice(hexString.Length - 48, 16), NumberStyles.HexNumber);
                    v3 = ulong.Parse(hexString.Slice(hexString.Length - 64, 16), NumberStyles.HexNumber);
                    break;
            }
        }



        public bool IsZero {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => v0 == 0 && v1 == 0 && v2 == 0 && v3 == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is U256 other && this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => v0.GetHashCode() ^ v1.GetHashCode() ^ v2.GetHashCode() ^ v3.GetHashCode();

        static ReadOnlySpan<byte> HexTemplate => new[] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' };

        [SkipLocalsInit]
        public override string ToString() {
            static void WriteHex(char* buffer, ulong v) {
                ref byte t = ref MemoryMarshal.GetReference(HexTemplate);
                byte* p = (byte*)&v;
                if (BitConverter.IsLittleEndian) {
                    buffer[2 * 7 + 0] = (char)Unsafe.Add(ref t, p[0] >> 4);
                    buffer[2 * 7 + 1] = (char)Unsafe.Add(ref t, p[0] & 15);
                    buffer[2 * 6 + 0] = (char)Unsafe.Add(ref t, p[1] >> 4);
                    buffer[2 * 6 + 1] = (char)Unsafe.Add(ref t, p[1] & 15);
                    buffer[2 * 5 + 0] = (char)Unsafe.Add(ref t, p[2] >> 4);
                    buffer[2 * 5 + 1] = (char)Unsafe.Add(ref t, p[2] & 15);
                    buffer[2 * 4 + 0] = (char)Unsafe.Add(ref t, p[3] >> 4);
                    buffer[2 * 4 + 1] = (char)Unsafe.Add(ref t, p[3] & 15);
                    buffer[2 * 3 + 0] = (char)Unsafe.Add(ref t, p[4] >> 4);
                    buffer[2 * 3 + 1] = (char)Unsafe.Add(ref t, p[4] & 15);
                    buffer[2 * 2 + 0] = (char)Unsafe.Add(ref t, p[5] >> 4);
                    buffer[2 * 2 + 1] = (char)Unsafe.Add(ref t, p[5] & 15);
                    buffer[2 * 1 + 0] = (char)Unsafe.Add(ref t, p[6] >> 4);
                    buffer[2 * 1 + 1] = (char)Unsafe.Add(ref t, p[6] & 15);
                    buffer[2 * 0 + 0] = (char)Unsafe.Add(ref t, p[7] >> 4);
                    buffer[2 * 0 + 1] = (char)Unsafe.Add(ref t, p[7] & 15);
                } else {
                    buffer[2 * 0 + 0] = (char)Unsafe.Add(ref t, p[0] >> 4);
                    buffer[2 * 0 + 1] = (char)Unsafe.Add(ref t, p[0] & 15);
                    buffer[2 * 1 + 0] = (char)Unsafe.Add(ref t, p[1] >> 4);
                    buffer[2 * 1 + 1] = (char)Unsafe.Add(ref t, p[1] & 15);
                    buffer[2 * 2 + 0] = (char)Unsafe.Add(ref t, p[2] >> 4);
                    buffer[2 * 2 + 1] = (char)Unsafe.Add(ref t, p[2] & 15);
                    buffer[2 * 3 + 0] = (char)Unsafe.Add(ref t, p[3] >> 4);
                    buffer[2 * 3 + 1] = (char)Unsafe.Add(ref t, p[3] & 15);
                    buffer[2 * 4 + 0] = (char)Unsafe.Add(ref t, p[4] >> 4);
                    buffer[2 * 4 + 1] = (char)Unsafe.Add(ref t, p[4] & 15);
                    buffer[2 * 5 + 0] = (char)Unsafe.Add(ref t, p[5] >> 4);
                    buffer[2 * 5 + 1] = (char)Unsafe.Add(ref t, p[5] & 15);
                    buffer[2 * 6 + 0] = (char)Unsafe.Add(ref t, p[6] >> 4);
                    buffer[2 * 6 + 1] = (char)Unsafe.Add(ref t, p[6] & 15);
                    buffer[2 * 7 + 0] = (char)Unsafe.Add(ref t, p[7] >> 4);
                    buffer[2 * 7 + 1] = (char)Unsafe.Add(ref t, p[7] & 15);
                }
            }

            char* buffer = stackalloc char[64];
            WriteHex(buffer, v3);
            WriteHex(buffer + 16, v2);
            WriteHex(buffer + 32, v1);
            WriteHex(buffer + 48, v0);
            return new string(buffer, 0, 64);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(in U256 left, in U256 right) => u256_less_than(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(in U256 left, in U256 right) => u256_less_than_equal(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(in U256 left, in U256 right) => u256_great_than(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(in U256 left, in U256 right) => u256_great_than_equal(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in U256 left, in U256 right) {
            return left.v0 == right.v0
                && left.v1 == right.v1
                && left.v2 == right.v2
                && left.v3 == right.v3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in U256 left, in U256 right) {
            return left.v0 != right.v0
                || left.v1 != right.v1
                || left.v2 != right.v2
                || left.v3 != right.v3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator U256(ulong value) => new U256(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> bytes, bool bigEndian = false) {
            if (bytes.Length < 32) throw new ArgumentException("缓冲区太小", nameof(bytes));
            if (!bigEndian) {
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(0, 8), v0);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(8, 8), v1);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(16, 8), v2);
                BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(24, 8), v3);
            } else {
                BinaryPrimitives.WriteUInt64BigEndian(bytes.Slice(0, 8), v3);
                BinaryPrimitives.WriteUInt64BigEndian(bytes.Slice(8, 8), v2);
                BinaryPrimitives.WriteUInt64BigEndian(bytes.Slice(16, 8), v1);
                BinaryPrimitives.WriteUInt64BigEndian(bytes.Slice(24, 8), v0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray(bool bigEndian = false) {
            var result = new byte[32];
            CopyTo(result, bigEndian);
            return result;
        }
    }
}
