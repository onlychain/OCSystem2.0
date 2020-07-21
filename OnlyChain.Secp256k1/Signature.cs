using OnlyChain.Secp256k1.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Secp256k1 {
    public sealed class Signature {
        internal U256 R, S;

        public void GetR(Span<byte> buffer) => R.CopyTo(buffer, bigEndian: true);
        public byte[] GetR() => R.ToArray(bigEndian: true);
        public void GetS(Span<byte> buffer) => S.CopyTo(buffer, bigEndian: true);
        public byte[] GetS() => S.ToArray(bigEndian: true);

        public Signature(in U256 r, in U256 s) => (R, S) = (r, s);

        static int ParseInt(ReadOnlySpan<byte> bytes, ref int index) {
            if (index >= bytes.Length) throw new InvalidSignatureException();
            int len = bytes[index];
            if (len < 128) {
                index++;
                if (index + len > bytes.Length) throw new InvalidSignatureException();
                return len;
            }
            int lenBytes = len ^ 0x80;
            if (lenBytes > 4 || bytes.Length - index < 1 + lenBytes)
                throw new InvalidSignatureException();
            int dataLen = bytes[index + 1];
            int dataOffset = index + 1 + lenBytes;
            for (int i = index + 2; i < dataOffset; i++) {
                dataLen = (dataLen << 8) | bytes[i];
            }
            if (bytes.Length - index < 1 + lenBytes + dataOffset)
                throw new InvalidSignatureException();
            index = dataOffset;
            return dataLen;
        }

        static U256 ParseU256(ReadOnlySpan<byte> bytes, ref int i) {
            if (bytes[i++] != 2) throw new InvalidSignatureException();
            int u256Len = ParseInt(bytes, ref i);
            Span<byte> tempBuffer = stackalloc byte[32];
            if (u256Len > 32) {
                while (bytes[i] == 0) {
                    u256Len--;
                    i++;
                }
                if (u256Len > 32) throw new InvalidSignatureException();
            }
            bytes.Slice(i, u256Len).CopyTo(tempBuffer.Slice(32 - u256Len, u256Len));
            i += u256Len;
            return new U256(tempBuffer, bigEndian: true);
        }

        public static Signature Parse(ReadOnlySpan<byte> bytes) {
            if (bytes.IsEmpty) throw new InvalidSignatureException("长度太小");
            int i = 0;
            if (bytes[i++] != 0x30) throw new InvalidSignatureException();
            ParseInt(bytes, ref i);

            var R = ParseU256(bytes, ref i);
            var S = ParseU256(bytes, ref i);
            if (R.IsZero || R >= ModN.N) throw new InvalidSignatureException();
            if (S.IsZero || S >= ModN.N) throw new InvalidSignatureException();
            return new Signature(R, S);
        }

        public void Serialize(Span<byte> buf) {
            if (buf.Length < 70) throw new ArgumentException("用于存放签名的缓冲区太小", nameof(buf));
            buf[0] = 0x30;
            buf[1] = 68;
            buf[2] = 2;
            buf[3] = 32;
            R.CopyTo(buf.Slice(4, 32), bigEndian: true);
            buf[36] = 2;
            buf[37] = 32;
            S.CopyTo(buf.Slice(38, 32), bigEndian: true);
        }

        public byte[] Serialize() {
            var result = new byte[70];
            Serialize(result);
            return result;
        }
    }
}
