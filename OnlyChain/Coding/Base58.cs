using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Numerics;

namespace OnlyChain.Coding {
    unsafe public static class Base58 {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint ReadUInt32BigEndian(ref byte first) {
            uint v = Unsafe.ReadUnaligned<uint>(ref first);
            if (BitConverter.IsLittleEndian) {
                return BinaryPrimitives.ReverseEndianness(v);
            } else {
                return v;
            }
        }

        static void ReadBigInteger(uint* num, ref int len, ReadOnlySpan<byte> bytes) {
            ref byte first = ref MemoryMarshal.GetReference(bytes);
            int uLen = bytes.Length / 4;
            for (int i = 1; i <= uLen; i++) {
                num[i - 1] = ReadUInt32BigEndian(ref Unsafe.Add(ref first, bytes.Length - i * 4));
            }
            int uRem = bytes.Length % 4;
            if (uRem != 0) {
                for (int i = 0; i < uRem; i++) {
                    num[uLen] = (num[uLen] << 8) | Unsafe.Add(ref first, i);
                }
            }
            while (len > 0 && num[len - 1] == 0) len--;
        }

        static uint DivRem58(uint* num, ref int len) {
            if (len == 0) return 0;

            long rem = 0;
            for (int i = len - 1; i >= 0; i--) {
                num[i] = (uint)Math.DivRem(num[i] | (rem << 32), 58, out rem);
            }
            if (num[len - 1] == 0) len--;
            return (uint)rem;
        }

        public static string Encode(ReadOnlySpan<byte> bytes) {
            ReadOnlySpan<byte> Table = new byte[] {
                (byte)'1',(byte)'2',(byte)'3',(byte)'4',(byte)'5',(byte)'6',(byte)'7',(byte)'8',(byte)'9',
                (byte)'A',(byte)'B',(byte)'C',(byte)'D',(byte)'E',(byte)'F',(byte)'G',(byte)'H',(byte)'J',(byte)'K',(byte)'L',(byte)'M',(byte)'N',(byte)'P',(byte)'Q',(byte)'R',(byte)'S',(byte)'T',(byte)'U',(byte)'V',(byte)'W',(byte)'X',(byte)'Y',(byte)'Z',
                (byte)'a',(byte)'b',(byte)'c',(byte)'d',(byte)'e',(byte)'f',(byte)'g',(byte)'h',(byte)'i',(byte)'j',(byte)'k',(byte)'m',(byte)'n',(byte)'o',(byte)'p',(byte)'q',(byte)'r',(byte)'s',(byte)'t',(byte)'u',(byte)'v',(byte)'w',(byte)'x',(byte)'y',(byte)'z'
            };

            int len = (bytes.Length + 3) / 4;
            uint* num = stackalloc uint[len];
            ReadBigInteger(num, ref len, bytes);

            int charsCap = ((bytes.Length * 11) >> 3) + 1;
            int charsLen = 0;
            char* base58Chars = stackalloc char[charsCap];
            ref byte table = ref MemoryMarshal.GetReference(Table);

            while (len != 0) {
                var rem = DivRem58(num, ref len);
                base58Chars[charsCap - ++charsLen] = (char)Unsafe.Add(ref table, (int)rem);
            }

            for (int i = 0; i < bytes.Length; i++) {
                if (bytes[i] != 0) break;
                base58Chars[charsCap - ++charsLen] = '1';
            }

            return new string(base58Chars, charsCap - charsLen, charsLen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUInt32BigEndian(ref byte first, uint value) {
            if (BitConverter.IsLittleEndian) {
                Unsafe.WriteUnaligned(ref first, BinaryPrimitives.ReverseEndianness(value));
            } else {
                Unsafe.WriteUnaligned(ref first, value);
            }
        }

        static void Mul58Add(uint* num, ref int len, uint add) {
            for (int i = 0; i < len; i++) {
                ulong x = (ulong)num[i] * 58 + add;
                num[i] = (uint)x;
                add = (uint)(x >> 32);
            }
            if (add > 0) num[len++] = add;
        }

        public static int Decode(ReadOnlySpan<char> base58Chars, Span<byte> outBytes) {
            ReadOnlySpan<byte> Table = new byte[] {
                0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
                0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
                0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
                0xff,0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0xff,0xff,0xff,0xff,0xff,0xff,
                0xff,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f,0x10,0xff,0x11,0x12,0x13,0x14,0x15,0xff,
                0x16,0x17,0x18,0x19,0x1a,0x1b,0x1c,0x1d,0x1e,0x1f,0x20,0xff,0xff,0xff,0xff,0xff,
                0xff,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2a,0x2b,0xff,0x2c,0x2d,0x2e,
                0x2f,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39, // 'z'
            };

            int lzcnt = 0;
            for (int i = 0; i < base58Chars.Length; i++) {
                if (base58Chars[i] != '1') break;
                lzcnt++;
            }

            ref char charFirst = ref MemoryMarshal.GetReference(base58Chars);
            ref byte table = ref MemoryMarshal.GetReference(Table);
            uint* num = stackalloc uint[(base58Chars.Length - lzcnt + 3) / 4];
            int len = 0;
            for (int i = lzcnt; i < base58Chars.Length; i++) {
                char c = Unsafe.Add(ref charFirst, i);
                if (c > 'z' || Unsafe.Add(ref table, c) == 0xff) throw new ArgumentException("出现意外的字符", nameof(base58Chars));
                Mul58Add(num, ref len, Unsafe.Add(ref table, c));
            }

            int uCount, uRem;
            if (len == 0 || num[len - 1] > 0xff_ff_ff) {
                uCount = len;
                uRem = 0;
            } else {
                uCount = len - 1;
                uRem = 4 - BitOperations.LeadingZeroCount(num[uCount]) / 8;
            }

            int outLength = uCount * 4 + uRem + lzcnt;
            if (outBytes.Length < outLength) throw new ArgumentException($"{nameof(outBytes)}空间不足", nameof(outBytes));

            ref byte @out = ref MemoryMarshal.GetReference(outBytes);
            for (int i = 1; i <= uCount; i++) {
                WriteUInt32BigEndian(ref Unsafe.Add(ref @out, outLength - i * 4), num[i - 1]);
            }
            for (int i = 0; i < uRem; i++) {
                Unsafe.Add(ref @out, lzcnt + i) = (byte)(num[uCount] >> ((uRem - i - 1) * 8));
            }
            for (int i = 0; i < lzcnt; i++) {
                Unsafe.Add(ref @out, i) = 0;
            }
            return outLength;
        }

        public static byte[] Decode(ReadOnlySpan<char> base58Chars) {
            Span<byte> buffer = stackalloc byte[base58Chars.Length];
            int count = Decode(base58Chars, buffer);
            return buffer[..count].ToArray();
        }
    }
}
