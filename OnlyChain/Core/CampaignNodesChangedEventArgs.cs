#nullable enable

using OnlyChain.Network;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OnlyChain.Core {
    public sealed class CampaignNodesChangedEventArgs : EventArgs {
        public SortedList<Bytes<Address>, SuperNode?> Old { get; }
        public SortedList<Bytes<Address>, SuperNode?> New { get; }

        public CampaignNodesChangedEventArgs(SortedList<Bytes<Address>, SuperNode?> old, SortedList<Bytes<Address>, SuperNode?> @new) {
            Old = old;
            New = @new;
        }
    }
}
