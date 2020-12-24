using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
#pragma warning disable CA1819 // Properties should not return arrays
    public readonly struct SortedCampaignNodeArray : IReadOnlyList<Bytes<Address>> {
        public Bytes<Address>[] Nodes { get; }
        public IComparer<Bytes<Address>> Comparer { get; }

        public int Count => Nodes.Length;

        public Bytes<Address> this[int index] => Nodes[index];


        public SortedCampaignNodeArray(Bytes<Address>[] nodes, IComparer<Bytes<Address>> comparer) {
            Nodes = nodes;
            Comparer = comparer;
        }

        public IEnumerator<Bytes<Address>> GetEnumerator() {
            return ((IEnumerable<Bytes<Address>>)Nodes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Nodes.GetEnumerator();
        }
    }
#pragma warning restore CA1819 // Properties should not return arrays
}
