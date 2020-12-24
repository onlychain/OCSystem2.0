#nullable enable

using OnlyChain.Network.Objects;
using System;
using System.IO;
using System.Net.Sockets;

namespace OnlyChain.Network {
    public sealed class GetValueEventArgs : EventArgs {
        public readonly BDict Dict;
        public readonly NetworkStream? Stream;

        public byte[]? Value { get; set; }

        public bool IsGetValue => Stream is not null;

        public GetValueEventArgs(BDict dict, NetworkStream? stream) {
            Dict = dict;
            Stream = stream;
        }
    }
}
