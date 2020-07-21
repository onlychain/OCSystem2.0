using System;
using System.IO;

namespace OnlyChain.Core {
    public static class Ex {
        public static ulong ReadVarUInt(this ReadOnlySpan<byte> data, out int readBytes) {
            ulong r = 0;
            int shift = 0;
            for (int j = 0; j < 8; j++, shift += 7) {
                byte v = data[j];
                r |= ((ulong)v & 0x7f) << shift;
                if ((v & 0x80) == 0) {
                    readBytes = j + 1;
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

        unsafe public static int WriteVarUInt(this Span<byte> buffer, ulong value) {
            byte* buf = stackalloc byte[9];
            int i = 0;
            while (i < 8) {
                buf[i] = (byte)(value & 0x7f);
                value >>= 7;
                if (value == 0) {
                    i++;
                    goto Result;
                }
                buf[i++] |= 0x80;
            }
            buf[i++] = (byte)value;
        Result:
            new ReadOnlySpan<byte>(buf, i).CopyTo(buffer);
            return i;
        }

        unsafe public static int WriteVarInt(this Span<byte> buffer, long value) {
            byte* buf = stackalloc byte[9];
            int i = 0;
            while (i < 8) {
                buf[i] = (byte)(value & 0x7f);
                value >>= 7;
                if ((value == 0 && (buf[i] & 0x40) == 0) || (value == -1 && (buf[i] & 0x40) != 0)) {
                    i++;
                    goto Result;
                }
                buf[i++] |= 0x80;
            }
            buf[i++] = (byte)value;
        Result:
            new ReadOnlySpan<byte>(buf, i).CopyTo(buffer);
            return i;
        }

        public static int WriteVarUInt(this Stream stream, ulong value) {
            Span<byte> buffer = stackalloc byte[9];
            int len = buffer.WriteVarUInt(value);
            stream.Write(buffer[..len]);
            return len;
        }

        public static int WriteVarInt(this Stream stream, long value) {
            Span<byte> buffer = stackalloc byte[9];
            int len = buffer.WriteVarInt(value);
            stream.Write(buffer[..len]);
            return len;
        }
    }
}
