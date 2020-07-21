using OnlyChain.Core;
using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OnlyChain.Network {
    public class RemoteResponse {
        public readonly ulong Index;
        public readonly Address Address;
        public readonly IPEndPoint Remote;
        public readonly BDict Response;

        public RemoteResponse(ulong index, in Address address, IPEndPoint remote, BDict response) {
            Index = index;
            Address = address;
            Remote = remote;
            Response = response;
        }
    }
}
