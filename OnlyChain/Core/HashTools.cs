using OnlyChain.Network.Objects;
using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    public static class HashTools {
        [SkipLocalsInit]
        unsafe public static Bytes<Address> ToAddress(this PublicKey publicKey) {
            Span<byte> buffer = stackalloc byte[sizeof(Secp256k1.Math.U256) * 2];
            publicKey.GetX(buffer);
            publicKey.GetY(buffer[sizeof(Secp256k1.Math.U256)..]);

            Bytes<Address> result;
            var hash256 = Sha256.ComputeHash(buffer);
            Ripemd160.ComputeHash(&hash256, &result);
            return result;
        }

        public static Bytes<Hash256> MessageHash(this ReadOnlySpan<byte> message) {
            return Sha256.DoubleHash(message);
        }

        public static Bytes<Hash256> MessageHash(this Span<byte> message) {
            return Sha256.DoubleHash(message);
        }

        public static Bytes<Hash256> HashesHash(this ReadOnlySpan<Bytes<Hash256>> hashes) {
            return Sha256.DoubleHash(MemoryMarshal.Cast<Bytes<Hash256>, byte>(hashes));
        }

        public static Bytes<Address> KeyToAddress(this ReadOnlySpan<byte> key) {
            return Ripemd160.ComputeHash(Sha256.ComputeHash(key)).ToBytes<Address>();
        }

        public static Bytes<Hash160> ToHash160(BDict dict) {
            using MemoryStream stream = new MemoryStream();
            BWriteArgs writeArgs = new BWriteArgs { Stream = stream, SortedKey = true };
            dict.Write(ref writeArgs);
            return Ripemd160.ComputeHash(Sha256.ComputeHash(stream.ToArray()));
        }
    }
}
