using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Drawing;

namespace OnlyChain.Core {
    unsafe public readonly struct Hash<TSize> : IEquatable<Hash<TSize>> where TSize : unmanaged {
        static Hash() {
            if (sizeof(TSize) % 4 != 0) throw new TypeLoadException("Hash长度必须是4的倍数");
        }

        static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        public static readonly Hash<TSize> Empty = new Hash<TSize>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TSize buffer;

        public Hash(ReadOnlySpan<byte> hash) {
            if (hash.Length != sizeof(TSize)) throw new ArgumentException($"必须是{sizeof(TSize)}字节", nameof(hash));

            buffer = Unsafe.As<byte, TSize>(ref MemoryMarshal.GetReference(hash));
        }

        public Hash(ReadOnlySpan<char> hash) {
            if (hash.Length != sizeof(TSize) * 2) throw new ArgumentException($"必须是{sizeof(TSize) * 2}字节", nameof(hash));

            buffer = Hex.Parse<TSize>(hash);
        }

        /// <summary>
        /// 栈上的<see cref="Hash{TSize}"/>对象使用此属性才是安全的。
        /// </summary>
        public readonly Span<byte> Span {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.As<TSize, byte>(ref Unsafe.AsRef(buffer)), sizeof(TSize));
        }

        /// <summary>
        /// 栈上的<see cref="Hash{TSize}"/>对象使用此属性才是安全的。
        /// </summary>
        public readonly ReadOnlySpan<byte> ReadOnlySpan {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TSize, byte>(ref Unsafe.AsRef(buffer)), sizeof(TSize));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Hash<TSize> FromSpan(Span<byte> span) {
            if (span.Length < sizeof(TSize)) throw new ArgumentException($"必须大于等于{sizeof(TSize)}字节", nameof(span));
            return ref Unsafe.As<byte, Hash<TSize>>(ref MemoryMarshal.GetReference(span));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hash<TSize> Random() {
            Hash<TSize> result;
            rng.GetBytes(new Span<byte>(&result, sizeof(TSize)));
            return result;
        }

        public readonly override string ToString() => Hex.ToString(this);

        public readonly override int GetHashCode() {
            return Unsafe.Add(ref Unsafe.As<TSize, int>(ref Unsafe.AsRef(buffer)), sizeof(TSize) / 4 - 1);
        }

        public readonly override bool Equals(object obj) => obj is Hash<TSize> other && Equals(other);

        public readonly bool Equals(Hash<TSize> other) {
            ref var @this = ref Unsafe.AsRef(buffer);
            for (int i = 0; i < sizeof(TSize) / 8; i++) {
                if (Unsafe.Add(ref Unsafe.As<TSize, ulong>(ref @this), i) != ((ulong*)&other)[i]) return false;
            }
            if (sizeof(TSize) % 8 != 0) {
                return Unsafe.Add(ref Unsafe.As<TSize, uint>(ref @this), sizeof(TSize) / 4 - 1) == ((uint*)&other)[sizeof(TSize) / 4 - 1];
            }
            return true;

        }

        public static bool operator ==(Hash<TSize> left, Hash<TSize> right) => left.Equals(right);
        public static bool operator !=(Hash<TSize> left, Hash<TSize> right) => !(left == right);

        public static implicit operator Hash<TSize>(string strHash) => new Hash<TSize>(strHash);

        public readonly void WriteToBytes(Span<byte> buffer) {
            ReadOnlySpan.CopyTo(buffer);
        }

        public readonly byte[] ToArray() {
            fixed (TSize* p = &buffer) {
                return new ReadOnlySpan<byte>(p, sizeof(TSize)).ToArray();
            }
        }
    }
}
