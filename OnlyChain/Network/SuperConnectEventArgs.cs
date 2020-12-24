using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class SuperConnectEventArgs : EventArgs {
        public SuperNode SuperNode { get; }


        public SuperConnectEventArgs(SuperNode superNode) {
            SuperNode = superNode;
        }
    }
}
