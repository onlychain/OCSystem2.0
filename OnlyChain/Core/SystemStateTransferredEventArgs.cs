using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class SystemStateTransferredEventArgs : EventArgs {
        public Block From { get; }
        public Block To { get; }

        public SystemStateTransferredEventArgs(Block from, Block to) {
            From = from;
            To = to;
        }
    }
}
