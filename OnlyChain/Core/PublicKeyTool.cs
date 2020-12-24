using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    unsafe public static class PublicKeyTool {
        public static byte[] ToArray(PublicKey sign) {
            var result = new byte[sizeof(Secp256k1.Math.U256) * 2];
            sign.X.CopyTo(result.AsSpan(0, sizeof(Secp256k1.Math.U256)), bigEndian: true);
            sign.Y.CopyTo(result.AsSpan(sizeof(Secp256k1.Math.U256), sizeof(Secp256k1.Math.U256)), bigEndian: true);
            return result;
        }

        public static PublicKey Parse(ReadOnlySpan<byte> buffer) {
            if (buffer.Length != sizeof(Secp256k1.Math.U256) * 2)
                throw new ArgumentException($"长度必须是{sizeof(Secp256k1.Math.U256) * 2}字节", nameof(buffer));

            return new PublicKey(
                new Secp256k1.Math.U256(buffer[..sizeof(Secp256k1.Math.U256)], bigEndian: true),
                new Secp256k1.Math.U256(buffer[sizeof(Secp256k1.Math.U256)..], bigEndian: true)
                );
        }
    }
}
