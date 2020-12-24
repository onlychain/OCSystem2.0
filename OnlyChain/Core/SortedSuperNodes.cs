#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Core {
    public sealed class SortedSuperNodes : IReadOnlyList<Bytes<Address>> {
        sealed class Comparer : IComparer<Bytes<Address>> {
            private readonly SortedSuperNodes source;

            public Comparer(SortedSuperNodes source) => this.source = source;

            unsafe public int Compare([AllowNull] Bytes<Address> x, [AllowNull] Bytes<Address> y) {
                if (x == y) return 0;
                ref readonly UserState xValue = ref source.mpt.GetRefValue(x);
                ref readonly UserState yValue = ref source.mpt.GetRefValue(y);
                if (xValue.IsNull() & yValue.IsNull()) return x.CompareTo(y);
                if (xValue.IsNull()) return 1;
                if (yValue.IsNull()) return -1;
                ulong xVotes = xValue.SuperPledge + xValue.Votes * 4;
                ulong yVotes = yValue.SuperPledge + yValue.Votes * 4;
                if (xVotes == yVotes) return x.CompareTo(y);
                return yVotes.CompareTo(xVotes);
            }
        }

        private MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt; // 用于获得得票数，排序依据
        private ImmutableSortedSet<Bytes<Address>> sortedAddresses;

        public SortedSuperNodes(MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt, IEnumerable<Bytes<Address>> superAddresses) {
            this.mpt = mpt;
            sortedAddresses = ImmutableSortedSet<Bytes<Address>>.Empty.WithComparer(new Comparer(this)).Intersect(superAddresses);
        }

        public void Update(MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt, IEnumerable<Bytes<Address>> changedAddresses) {
            var builder = sortedAddresses.ToBuilder();
            builder.ExceptWith(changedAddresses);
            this.mpt = mpt;
            builder.IntersectWith(changedAddresses);
            sortedAddresses = builder.ToImmutable();
        }

        public Bytes<Address> this[int index] => sortedAddresses[index];

        public int Count => sortedAddresses.Count;

        public IEnumerator<Bytes<Address>> GetEnumerator() => sortedAddresses.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
