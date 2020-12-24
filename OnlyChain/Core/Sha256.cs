using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

namespace OnlyChain.Core {
    public static class Sha256 {
        [SkipLocalsInit]
        unsafe public static Bytes<Hash256> ComputeHash(ReadOnlySpan<byte> data) {
            using SHA256 sha256 = SHA256.Create();
            Bytes<Hash256> result;
            sha256.TryComputeHash(data, new Span<byte>(&result, sizeof(Hash256)), out _);
            return result;
        }

        [SkipLocalsInit]
        unsafe public static Bytes<Hash256> DoubleHash(ReadOnlySpan<byte> data) {
            using SHA256 sha256 = SHA256.Create();
            Bytes<Hash256> result;
            sha256.TryComputeHash(data, new Span<byte>(&result, sizeof(Hash256)), out _);
            sha256.TryComputeHash(new ReadOnlySpan<byte>(&result, sizeof(Hash256)), new Span<byte>(&result, sizeof(Hash256)), out _);
            return result;
        }

        public static byte[] ComputeHashToArray(ReadOnlySpan<byte> data) {
            using SHA256 sha256 = SHA256.Create();
            var result = new byte[32];
            sha256.TryComputeHash(data, result, out _);
            return result;
        }
    }
}
