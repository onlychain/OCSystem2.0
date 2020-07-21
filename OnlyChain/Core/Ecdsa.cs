using OnlyChain.Secp256k1;
using System;

namespace OnlyChain.Core {
    unsafe public static class Ecdsa {
        public static Signature Sign(ReadOnlySpan<byte> privateKey, Hash<Size256> messageHash) {
            return Secp256k1.Secp256k1.Sign(privateKey, new ReadOnlySpan<byte>(&messageHash, sizeof(Hash<Size256>)));
        }

        public static bool Verify(PublicKey publicKey, Hash<Size256> messageHash, Signature signature) {
            return Secp256k1.Secp256k1.Verify(publicKey, new ReadOnlySpan<byte>(&messageHash, sizeof(Hash<Size256>)), signature);
        }

        public static Signature Sign(in this Serializer serializer, ReadOnlySpan<byte> privateKey, out Hash<Size256> hash) {
            hash = serializer.Data[..serializer.Index].MessageHash();
            return Sign(privateKey, hash);
        }
    }
}
