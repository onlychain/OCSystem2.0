using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlyChain.Core {
    public sealed class Blacklist<T> : HashSet<T> {
        public int MaxCount { get; }

        public Blacklist(int maxCount) {
            if (maxCount < 1) throw new ArgumentOutOfRangeException(nameof(maxCount));
            MaxCount = maxCount;
        }

        public new bool Add(T value) {
            if (base.Add(value)) {
                if (Count > MaxCount) Remove(this.First());
                return true;
            }
            return false;
        }
    }
}
