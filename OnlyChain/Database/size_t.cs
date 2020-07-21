#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace OnlyChain.Database {
#pragma warning disable IDE1006 // 命名样式
    [DebuggerDisplay("{(ulong)value}"), DebuggerStepThrough]
    unsafe internal readonly struct size_t : IFormattable, IComparable<size_t>, IEquatable<size_t> {
        public static readonly size_t MinValue = new size_t();
        public static readonly size_t MaxValue = new size_t(ulong.MaxValue);

        private readonly void* value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public size_t(UIntPtr value) => this.value = (void*)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public size_t(ulong value) => this.value = (void*)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public size_t(int value) {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            this.value = (void*)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(size_t @this) => (int)@this.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(size_t @this) => (uint)@this.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(size_t @this) => (ulong)@this.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(ulong value) => new size_t(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(uint value) => new size_t(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator size_t(int value) => new size_t((ulong)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString() => ((ulong)value).ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string? format, IFormatProvider? formatProvider)
            => ((ulong)value).ToString(format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(size_t other) => ((ulong)value).CompareTo((ulong)other.value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object? obj) => obj is size_t other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => (int)value ^ (int)((ulong)value >> 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(size_t other) => value == other.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(size_t left, size_t right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(size_t left, size_t right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(size_t left, size_t right) => left.value < right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(size_t left, size_t right) => left.value <= right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(size_t left, size_t right) => left.value > right.value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(size_t left, size_t right) => left.value >= right.value;
    }
#pragma warning restore IDE1006 // 命名样式
}
