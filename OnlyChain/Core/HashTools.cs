using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    public static class HashTools {
        unsafe public static Address ToAddress(this PublicKey publicKey) {
            Span<byte> buffer = stackalloc byte[sizeof(U256) * 2];
            publicKey.GetX(buffer);
            publicKey.GetY(buffer[sizeof(U256)..]);

            Address result;
            var hash256 = Sha256.ComputeHash(buffer);
            Ripemd160.ComputeHash(&hash256, &result);
            return result;
        }

        public static Hash<Size256> MessageHash(this ReadOnlySpan<byte> message) {
            return Sha256.ComputeHash(message);
        }

        public static Hash<Size256> MessageHash(this Span<byte> message) {
            return Sha256.ComputeHash(message);
        }

        public static Hash<Size256> HashesHash(this ReadOnlySpan<Hash<Size256>> hashes) {
            return Sha256.ComputeHash(MemoryMarshal.Cast<Hash<Size256>, byte>(hashes));
        }

        public static Address KeyToAddress(this ReadOnlySpan<byte> key) {
            return Ripemd160.ComputeHash(Sha256.ComputeHash(key));
        }
    }
}
