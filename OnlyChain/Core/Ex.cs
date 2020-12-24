using System;
using System.Buffers.Binary;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace OnlyChain.Core {
    public static class Ex {
        public static ulong ReadVarUInt(this ReadOnlySpan<byte> data, out int readBytes) {
            ulong r = 0;
            int shift = 0;
            for (int i = 0; i < 8; i++, shift += 7) {
                byte v = data[i];
                r |= ((ulong)v & 0x7f) << shift;
                if ((v & 0x80) == 0) {
                    readBytes = i + 1;
                    return r;
                }
            }
            r |= (ulong)data[8] << 56;
            readBytes = 9;
            return r;
        }

        public static long ReadVarInt(this ReadOnlySpan<byte> data, out int readBytes) {
            ulong r = ReadVarUInt(data, out readBytes);
            if (readBytes == 9) return (long)r;
            int shift = 64 - readBytes * 7;
            return (long)r << shift >> shift;
        }

        public static ulong ReadVarUInt(this Span<byte> data, out int readBytes) => ReadVarUInt((ReadOnlySpan<byte>)data, out readBytes);
        public static long ReadVarInt(this Span<byte> data, out int readBytes) => ReadVarInt((ReadOnlySpan<byte>)data, out readBytes);

        public static ulong ReadVarUInt(this Stream stream, out int readBytes) {
            ulong r = 0;
            int shift = 0;
            int v;
            for (int j = 0; j < 8; j++, shift += 7) {
                v = stream.ReadByte();
                if (v < 0) throw new EndOfStreamException();
                r |= ((ulong)v & 0x7f) << shift;
                if ((v & 0x80) == 0) {
                    readBytes = j + 1;
                    return r;
                }
            }

            v = stream.ReadByte();
            if (v < 0) throw new EndOfStreamException();
            r |= (ulong)v << 56;
            readBytes = 9;
            return r;
        }

        public static long ReadVarInt(this Stream stream, out int readBytes) {
            ulong r = ReadVarUInt(stream, out readBytes);
            if (readBytes == 9) return (long)r;
            int shift = 64 - readBytes * 7;
            return (long)r << shift >> shift;
        }


        unsafe private static int WriteVarUInt(byte* buf, ulong value) {
            int i = 0;
            while (i < 8) {
                buf[i] = (byte)(value & 0x7f);
                value >>= 7;
                if (value == 0) {
                    i++;
                    return i;
                }
                buf[i++] |= 0x80;
            }
            buf[i++] = (byte)value;
            return i;
        }

        unsafe private static int WriteVarInt(byte* buf, long value) {
            int i = 0;
            while (i < 8) {
                buf[i] = (byte)(value & 0x7f);
                value >>= 7;
                if ((value == 0 && (buf[i] & 0x40) == 0) || (value == -1 && (buf[i] & 0x40) != 0)) {
                    i++;
                    return i;
                }
                buf[i++] |= 0x80;
            }
            buf[i++] = (byte)value;
            return i;
        }

        [SkipLocalsInit]
        unsafe public static int WriteVarUInt(this Span<byte> buffer, ulong value) {
            byte* buf = stackalloc byte[9];
            int len = WriteVarUInt(buf, value);
            new ReadOnlySpan<byte>(buf, len).CopyTo(buffer);
            return len;
        }

        [SkipLocalsInit]
        unsafe public static int WriteVarInt(this Span<byte> buffer, long value) {
            byte* buf = stackalloc byte[9];
            int len = WriteVarInt(buf, value);
            new ReadOnlySpan<byte>(buf, len).CopyTo(buffer);
            return len;
        }

        [SkipLocalsInit]
        unsafe public static int WriteVarUInt(this Stream stream, ulong value) {
            byte* buf = stackalloc byte[9];
            int len = WriteVarUInt(buf, value);
            stream.Write(new ReadOnlySpan<byte>(buf, len));
            return len;
        }

        [SkipLocalsInit]
        unsafe public static int WriteVarInt(this Stream stream, long value) {
            byte* buf = stackalloc byte[9];
            int len = WriteVarInt(buf, value);
            stream.Write(new ReadOnlySpan<byte>(buf, len));
            return len;
        }

        public static void RandomShuffle<T>(this Span<T> span) {
            var random = new Random();
            for (int i = 0; i < span.Length - 1; i++) {
                int j = random.Next(i, span.Length);
                T temp = span[i];
                span[i] = span[j];
                span[j] = temp;
            }
        }

        unsafe public static bool TryGetUInt64(this Bytes<U256> bytes, out ulong value) {
            var buffer = new ReadOnlySpan<byte>(&bytes, sizeof(U256));
            if (!buffer[..^sizeof(ulong)].SequenceEqual(default)) {
                value = default;
                return false;
            }
            value = BinaryPrimitives.ReadUInt64BigEndian(buffer[^sizeof(ulong)..]);
            return true;
        }
    }
}
