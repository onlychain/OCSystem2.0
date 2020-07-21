#nullable enable

using OnlyChain.Network;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OnlyChain.Core {
    public sealed class CampaignNodesChangedEventArgs : EventArgs {
        public SortedList<Address, SuperNode?> Old { get; }
        public SortedList<Address, SuperNode?> New { get; }

        public CampaignNodesChangedEventArgs(SortedList<Address, SuperNode?> old, SortedList<Address, SuperNode?> @new) {
            Old = old;
            New = @new;
        }
    }
}
