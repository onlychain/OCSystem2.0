using OnlyChain.Core;
using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class RemoteRequest {
        public BDict Request { get; }
        public IPEndPoint Remote { get; }
        public Address Address { get; }

        internal RemoteRequest(BDict request, IPEndPoint remote, in Address senderAddress) {
            Request = request;
            Remote = remote;
            Address = senderAddress;
        }
    }
}
