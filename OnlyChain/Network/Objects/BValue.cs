using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("{Value}")]
    public abstract class BValue<T> : BObject {
        public readonly T Value;

        protected BValue(T value) => Value = value;
        protected BValue(in T value) => Value = value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj)
            => (obj is BValue<T> other && Value.Equals(other.Value))
            || (obj is T otherValue && Value.Equals(otherValue));

        public override string ToString() => Value.ToString();

        public static implicit operator T(BValue<T> @this) => @this.Value;

        public void Deconstruct(out T result) => result = Value;

        protected new abstract void Write(Stream stream);

        public override void Write(ref BWriteArgs args) => Write(args.Stream);
    }
}
