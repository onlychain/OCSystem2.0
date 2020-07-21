using System;
using System.Security.Cryptography;

namespace OnlyChain.Core {
    public static class Sha256 {
        private static readonly SHA256 sha256 = SHA256.Create();

        unsafe public static Hash<Size256> ComputeHash(ReadOnlySpan<byte> data) {
            Hash<Size256> result;
            sha256.TryComputeHash(data, new Span<byte>(&result, sizeof(Hash<Size256>)), out _);
            return result;
        }

        public static byte[] ComputeHashToArray(ReadOnlySpan<byte> data) {
            var result = new byte[32];
            sha256.TryComputeHash(data, result, out _);
            return result;
        }
    }
}
