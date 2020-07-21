using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Network {
    public sealed class SuperNodeEventArgs : EventArgs {
        public SuperNode SuperNode { get; }
        public ReadOnlyMemory<byte> Data { get; }

        public SuperNodeEventArgs(SuperNode superNode, ReadOnlyMemory<byte> data) {
            SuperNode = superNode;
            Data = data;
        }
    }
}
