using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OnlyChain.Core {
    [DebuggerDisplay("{ToString()}")]
    unsafe public readonly struct Bytes<T> : IEquatable<Bytes<T>>, IComparable<Bytes<T>> where T : unmanaged {
        static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public static readonly Bytes<T> Empty = default;
        public static readonly Bytes<T> Max = GetMax();
        public static readonly int Size = sizeof(T);

        [SkipLocalsInit]
        static Bytes<T> GetMax() {
            Bytes<T> result;
            new Span<byte>(&result, sizeof(T)).Fill(0xff);
            return result;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly T buffer;

        public Bytes(ReadOnlySpan<byte> bytes) {
            if (bytes.Length != sizeof(T)) throw new ArgumentException($"必须是{sizeof(T)}字节", nameof(bytes));

            buffer = Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(bytes));
        }

        public Bytes(ReadOnlySpan<char> hexBytes) {
            if (hexBytes.Length != sizeof(T) * 2) throw new ArgumentException($"必须是{sizeof(T) * 2}字节", nameof(hexBytes));

            buffer = Hex.Parse<T>(hexBytes);
        }

        [SkipLocalsInit]
        public Bytes(ulong value) {
            Span<byte> temp = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(temp, value);

            fixed (T* p = &buffer) {
                if (sizeof(T) < sizeof(ulong)) {
                    temp[(sizeof(ulong) - sizeof(T))..].CopyTo(new Span<byte>(p, sizeof(T)));
                } else {
                    new Span<byte>(p, sizeof(T) - sizeof(ulong)).Clear();
                    temp.CopyTo(new Span<byte>((byte*)p + (sizeof(T) - sizeof(ulong)), sizeof(ulong)));
                }
            }
        }

        /// <summary>
        /// 栈上的<see cref="Bytes{T}"/>对象使用此属性才是安全的。
        /// </summary>
        public readonly Span<byte> Span {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(buffer)), sizeof(T));
        }

        /// <summary>
        /// 栈上的<see cref="Bytes{T}"/>对象使用此属性才是安全的。
        /// </summary>
        public readonly ReadOnlySpan<byte> ReadOnlySpan {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(buffer)), sizeof(T));
        }

        public static ref Bytes<T> FromSpan(Span<byte> span) {
            if (span.Length < sizeof(T)) throw new ArgumentException($"必须大于等于{sizeof(T)}字节", nameof(span));
            return ref Unsafe.As<byte, Bytes<T>>(ref MemoryMarshal.GetReference(span));
        }

        public static ref readonly Bytes<T> FromSpan(ReadOnlySpan<byte> span) {
            if (span.Length < sizeof(T)) throw new ArgumentException($"必须大于等于{sizeof(T)}字节", nameof(span));
            return ref Unsafe.As<byte, Bytes<T>>(ref MemoryMarshal.GetReference(span));
        }

        [SkipLocalsInit]
        public static Bytes<T> Random() {
            Bytes<T> result;
            rng.GetBytes(new Span<byte>(&result, sizeof(T)));
            return result;
        }

        public readonly override string ToString() => Hex.ToString(this);

        public readonly override int GetHashCode() {
            int hash = 0;
            fixed (T* p = &buffer) {
                if (sizeof(T) % 4 == 0) {
                    for (int i = 0; i < sizeof(T) / 4; i++) {
                        hash = unchecked((int)BitOperations.RotateLeft((uint)hash, 7)) ^ ((int*)p)[i];
                    }
                } else {
                    for (int i = 0; i < sizeof(T); i++) {
                        hash = unchecked((int)BitOperations.RotateLeft((uint)hash, 7)) ^ ((byte*)p)[i];
                    }
                }
            }
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equals(Bytes<T> left, Bytes<T> right) {
            if (sizeof(T) == 32) {
                return ((long*)&left)[0] == ((long*)&right)[0]
                    && ((long*)&left)[1] == ((long*)&right)[1]
                    && ((long*)&left)[2] == ((long*)&right)[2]
                    && ((long*)&left)[3] == ((long*)&right)[3];
            }
            if (sizeof(T) == 20) {
                return ((long*)&left)[0] == ((long*)&right)[0]
                    && ((long*)&left)[1] == ((long*)&right)[1]
                    && ((int*)&left)[4] == ((int*)&right)[4];
            }

            unchecked {
                for (int i = 0; i < sizeof(T) / 8; i++) {
                    if (((long*)&left)[i] != ((long*)&right)[i]) return false;
                }
                if ((sizeof(T) & 4) != 0) {
                    if (((int*)&left)[sizeof(T) / 4 - 1] != ((int*)&right)[sizeof(T) / 4 - 1]) return false;
                }
                if ((sizeof(T) & 2) != 0) {
                    if (((short*)&left)[sizeof(T) / 2 - 1] != ((short*)&right)[sizeof(T) / 2 - 1]) return false;
                }
                if ((sizeof(T) & 1) != 0) {
                    if (((byte*)&left)[sizeof(T) - 1] != ((byte*)&right)[sizeof(T) - 1]) return false;
                }
                return true;
            }
        }

        public readonly override bool Equals(object obj) => obj is Bytes<T> other && Equals(this, other);

        public readonly bool Equals(Bytes<T> other) => Equals(this, other);

        public readonly int CompareTo(Bytes<T> other) {
            fixed (T* @this = &buffer) {
                return new ReadOnlySpan<byte>(@this, sizeof(T)).SequenceCompareTo(new ReadOnlySpan<byte>(&other, sizeof(T)));
            }
        }


        public static bool operator ==(Bytes<T> left, Bytes<T> right) => Equals(left, right);
        public static bool operator !=(Bytes<T> left, Bytes<T> right) => !Equals(left, right);
        public static bool operator <(Bytes<T> left, Bytes<T> right) => left.CompareTo(right) < 0;
        public static bool operator <=(Bytes<T> left, Bytes<T> right) => left.CompareTo(right) <= 0;
        public static bool operator >(Bytes<T> left, Bytes<T> right) => left.CompareTo(right) > 0;
        public static bool operator >=(Bytes<T> left, Bytes<T> right) => left.CompareTo(right) >= 0;

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bytes<T> operator ^(Bytes<T> left, Bytes<T> right) {
            Unsafe.SkipInit(out Bytes<T> result);

            if (sizeof(T) == 32) {
                ((long*)&result)[0] = ((long*)&left)[0] ^ ((long*)&right)[0];
                ((long*)&result)[1] = ((long*)&left)[1] ^ ((long*)&right)[1];
                ((long*)&result)[2] = ((long*)&left)[2] ^ ((long*)&right)[2];
                ((long*)&result)[3] = ((long*)&left)[3] ^ ((long*)&right)[3];
                return result;
            }
            if (sizeof(T) == 20) {
                ((long*)&result)[0] = ((long*)&left)[0] ^ ((long*)&right)[0];
                ((long*)&result)[1] = ((long*)&left)[1] ^ ((long*)&right)[1];
                ((int*)&result)[4] = ((int*)&left)[4] ^ ((int*)&right)[4];
                return result;
            }

            for (int i = 0; i < sizeof(T) / 8; i++) {
                ((long*)&result)[i] = ((long*)&left)[i] ^ ((long*)&right)[i];
            }
            if ((sizeof(T) & 4) != 0) {
                ((int*)&result)[sizeof(T) / 4 - 1] = ((int*)&left)[sizeof(T) / 4 - 1] ^ ((int*)&right)[sizeof(T) / 4 - 1];
            }
            if ((sizeof(T) & 2) != 0) {
                ((short*)&result)[sizeof(T) / 2 - 1] = unchecked((short)(((short*)&left)[sizeof(T) / 2 - 1] ^ ((short*)&right)[sizeof(T) / 2 - 1]));
            }
            if ((sizeof(T) & 1) != 0) {
                ((byte*)&result)[sizeof(T) - 1] = unchecked((byte)(((byte*)&left)[sizeof(T) - 1] ^ ((byte*)&right)[sizeof(T) - 1]));
            }
            return result;
        }

        /// <summary>
        /// 检测对应下标的二进制位是否为1。
        /// </summary>
        /// <param name="bitIndex">从低位到高位（从右到左）的索引，从0开始计数。</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Bit(int bitIndex) {
            if (bitIndex < 0 || bitIndex >= sizeof(T) * 8) throw new ArgumentOutOfRangeException(nameof(bitIndex));
            fixed (T* p = &buffer) {
                return (((byte*)p)[sizeof(T) - 1 - (bitIndex >> 3)] & (1 << (bitIndex & 7))) != 0;
            }
        }

        public readonly int Log2 {
            get {
                fixed (T* p = &buffer) {
                    int result = sizeof(T) * 8 - 1;
                    if (BitConverter.IsLittleEndian) {
                        for (int i = 0; i < sizeof(T) / 8; i++) {
                            int t = BitOperations.LeadingZeroCount(BinaryPrimitives.ReverseEndianness(((ulong*)p)[i]));
                            result -= t;
                            if (t != 64) return result;
                        }
                        if ((sizeof(T) & 4) != 0) {
                            int t = BitOperations.LeadingZeroCount(BinaryPrimitives.ReverseEndianness(((uint*)p)[sizeof(T) / 4 - 1]));
                            result -= t;
                            if (t != 32) return result;
                        }
                        if ((sizeof(T) & 2) != 0) {
                            int t = BitOperations.LeadingZeroCount(BinaryPrimitives.ReverseEndianness(((ushort*)p)[sizeof(T) / 2 - 1])) - 16;
                            result -= t;
                            if (t != 16) return result;
                        }
                        if ((sizeof(T) & 1) != 0) {
                            int t = BitOperations.LeadingZeroCount(BinaryPrimitives.ReverseEndianness(((byte*)p)[sizeof(T) - 1])) - 24;
                            result -= t;
                        }
                    } else {
                        for (int i = 0; i < sizeof(T) / 8; i++) {
                            int t = BitOperations.LeadingZeroCount(((ulong*)p)[i]);
                            result -= t;
                            if (t != 64) return result;
                        }
                        if ((sizeof(T) & 4) != 0) {
                            int t = BitOperations.LeadingZeroCount(((uint*)p)[sizeof(T) / 4 - 1]);
                            result -= t;
                            if (t != 32) return result;
                        }
                        if ((sizeof(T) & 2) != 0) {
                            int t = BitOperations.LeadingZeroCount(((ushort*)p)[sizeof(T) / 2 - 1]) - 16;
                            result -= t;
                            if (t != 16) return result;
                        }
                        if ((sizeof(T) & 1) != 0) {
                            int t = BitOperations.LeadingZeroCount(((byte*)p)[sizeof(T) - 1]) - 24;
                            result -= t;
                        }
                    }
                    return result;
                }
            }
        }

        public static implicit operator Bytes<T>(string hexBytes) => new Bytes<T>(hexBytes);
        public static implicit operator Bytes<T>(ulong value) => new Bytes<T>(value);


        public readonly void CopyTo(Span<byte> buffer) {
            fixed (T* @this = &this.buffer) {
                new ReadOnlySpan<byte>(@this, sizeof(T)).CopyTo(buffer);
            }
        }

        public readonly byte[] ToArray() {
            fixed (T* p = &buffer) {
                return new ReadOnlySpan<byte>(p, sizeof(T)).ToArray();
            }
        }

        public readonly IComparer<Bytes<T>> Comparer {
            get {
                static Comparison<Bytes<T>> Comparer(Bytes<T> @this) => (a, b) => (a ^ @this).CompareTo(b ^ @this);
                return Comparer<Bytes<T>>.Create(Comparer(this));
            }
        }

        [SkipLocalsInit]
        private readonly TOther To<TOther>() where TOther : unmanaged {
            if (sizeof(T) != sizeof(TOther)) throw new InvalidCastException();

            TOther result;
            CopyTo(new Span<byte>(&result, sizeof(TOther)));
            return result;
        }

        public readonly Bytes<TOther> ToBytes<TOther>() where TOther : unmanaged => To<Bytes<TOther>>();
    }

    public static class Bytes {
        unsafe public static bool Equals<T1, T2>(Bytes<T1> left, Bytes<T2> right) where T1 : unmanaged where T2 : unmanaged {
            if (sizeof(T1) != sizeof(T2)) throw new ArgumentException($"{nameof(left)}和{nameof(right)}长度不一致");
            return left == right.ToBytes<T1>();
        }

        unsafe public static int Compare<T1, T2>(Bytes<T1> left, Bytes<T2> right) where T1 : unmanaged where T2 : unmanaged {
            if (sizeof(T1) != sizeof(T2)) throw new ArgumentException($"{nameof(left)}和{nameof(right)}长度不一致");
            return new ReadOnlySpan<byte>(&left, sizeof(T1)).SequenceCompareTo(new ReadOnlySpan<byte>(&right, sizeof(T2)));
        }
    }
}
