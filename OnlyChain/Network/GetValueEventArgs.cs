using OnlyChain.Network.Objects;
using System;

namespace OnlyChain.Network {
    public sealed class GetValueEventArgs : EventArgs {
        public readonly Node Sender;
        public readonly byte[] Key;
        public bool HasValue { get; set; } = false;

        public GetValueEventArgs(Node sender, byte[] key) {
            Sender = sender;
            Key = key;
        }
    }
}
