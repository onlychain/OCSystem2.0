using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class StructRef<T> where T : struct {
        public T Value;

        public StructRef() { }

        public StructRef(in T value) => Value = value;

        public static implicit operator StructRef<T>(in T value) => new StructRef<T>(value);
    }
}
