using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class SuperEventArgs : EventArgs {
        public readonly NetworkStream Stream;
        public readonly BDict Dict;
        public readonly SuperNode Node;

        public SuperEventArgs(NetworkStream stream, BDict dict, SuperNode node) {
            Stream = stream;
            Dict = dict;
            Node = node;
        }
    }
}
