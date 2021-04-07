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
        string? Name { get; }
        string? NetworkPrefix { get; }
        Bytes<Address> Address { get; }
        KBucket Nodes { get; }
        BlockChainSystem System { get; }
        IPEndPoint EndPoint { get; }
        IPEndPoint BindEndPoint { get; }
        P2P P2P { get; }
        CancellationToken CloseCancellationToken { get; }
        SuperNodeConfig? SuperConfig { get; }

        Task<Node?> GetTargetNode(Bytes<Address> address);

        void Log(object message);
    }
}
