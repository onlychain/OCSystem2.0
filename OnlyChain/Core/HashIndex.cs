using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public readonly struct HashIndex {
        public readonly int Value;

        public HashIndex(int value) => Value = value;

        public override string ToString() => Value.ToString();

        public static implicit operator int(HashIndex index) => index.Value;
        public static implicit operator HashIndex(int index) => new HashIndex(index);
    }
}
