using System;

namespace OnlyChain.Core {
    public readonly ref struct SecuritySpan<T> {
        public readonly Span<T> Data;

        public SecuritySpan(Span<T> data) => Data = data;

        public void Dispose() => Data.Clear();

        public static implicit operator Span<T>(SecuritySpan<T> span) => span.Data;
        public static implicit operator ReadOnlySpan<T>(SecuritySpan<T> span) => span.Data;
    }
}
