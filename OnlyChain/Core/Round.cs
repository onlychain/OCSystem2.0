using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OnlyChain.Core {
    [System.Diagnostics.DebuggerDisplay("({Rounds}, {IndexInRound})")]
    public readonly struct Round : IEquatable<Round> {
        public static readonly Round InvalidValue = new Round(0, 0);

        public readonly int Rounds;
        public readonly int IndexInRound;

        public bool IsValid => Rounds > 0;

        public Round(int rounds, int indexInRound) => (Rounds, IndexInRound) = (rounds, indexInRound);

        public override bool Equals(object obj) {
            return obj is Round other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Rounds, IndexInRound);
        }

        public bool Equals([AllowNull] Round other) {
            return Rounds == other.Rounds && IndexInRound == other.IndexInRound;
        }

        public static bool operator ==(Round left, Round right) {
            return left.Equals(right);
        }

        public static bool operator !=(Round left, Round right) {
            return !(left == right);
        }
    }
}
