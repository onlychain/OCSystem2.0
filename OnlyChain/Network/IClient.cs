#nullable enable

using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public interface IClient {
        string? NetworkPrefix { get; }
        Address Address { get; }
        KBucket Nodes { get; }
        BlockChainSystem System { get; }
        IPEndPoint UdpEndPoint { get; }
        IPEndPoint TcpEndPoint { get; }

        event EventHandler<BroadcastEventArgs> ReceiveBroadcast;

        ValueTask<Node?> Lookup(Address target, int nodePoolSize = 20, CancellationToken cancellationToken = default);
    }
}
