using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OnlyChain.Secp256k1.Math;

namespace OnlyChain.Secp256k1 {
    public sealed class PublicKey {
        const byte EvenPublicKey = 2;
        const byte OddPublicKey = 3;
        const byte FullPublicKey = 4;

        public readonly U256 X, Y;

        public PublicKey(in U256 x, in U256 y) => (X, Y) = (x, y);

        public void GetX(Span<byte> buffer) => X.CopyTo(buffer, true);
        public byte[] GetX() => X.ToArray(true);
        public void GetY(Span<byte> buffer) => Y.CopyTo(buffer, true);
        public byte[] GetY() => Y.ToArray(true);

        public int Serialize(Span<byte> buf, bool compressed = false) {
            int len = compressed ? 33 : 65;
            if (buf.Length < len) throw new ArgumentOutOfRangeException(nameof(buf));
            X.CopyTo(buf.Slice(1, 32), true);
            if (!compressed) {
                Y.CopyTo(buf.Slice(33, 32), true);
                buf[0] = FullPublicKey;
                return 65;
            } else {
                buf[0] = Y.v0 % 2 == 0 ? EvenPublicKey : OddPublicKey;
                return 33;
            }
        }

        public byte[] Serialize(bool compressed = false) {
            var result = new byte[compressed ? 33 : 65];
            Serialize(result, compressed);
            return result;
        }

        public static PublicKey Parse(ReadOnlySpan<byte> bytes, out int readBytes) {
            if (bytes.Length < 33) throw new InvalidPublicKeyException("长度太小");
            U256P x = new U256(bytes.Slice(1, 32), bigEndian: true), y;
            if (x.Value >= U256P.P) throw new InvalidPublicKeyException();
            if (bytes[0] == EvenPublicKey || bytes[0] == OddPublicKey) {
                try {
                    y = EllipticCurve.GetY(x);
                    if ((y.Value.v0 % 2 == 0) != (bytes[0] == EvenPublicKey)) {
                        // y.v0 % 2 == 0 && bytes[0] == OddPublicKey
                        // y.v0 % 2 != 0 && bytes[0] == EvenPublicKey
                        y = -y;
                    }
                    readBytes = 33;
                    return new PublicKey(x, y);
                } catch (ArgumentException) {
                    throw new InvalidPublicKeyException();
                }
            } else if (bytes[0] == FullPublicKey) {
                if (bytes.Length < 65) throw new InvalidPublicKeyException("长度太小");
                y = new U256(bytes.Slice(33, 32), bigEndian: true);
                if ((x ^ 3) + 7 != (y ^ 2)) {
                    throw new InvalidPublicKeyException();
                }
                readBytes = 65;
                return new PublicKey(x, y);
            } else {
                throw new InvalidPublicKeyException("不支持的公钥类型");
            }
        }

        public static PublicKey Parse(ReadOnlySpan<byte> bytes) => Parse(bytes, out _);
    }
}
