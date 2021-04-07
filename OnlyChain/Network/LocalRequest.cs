using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class LocalRequest {
        internal TaskCompletionSource<RemoteResponse> Response { get; }
        public ulong Index { get; }
        public BDict Request { get; }
        public TimeSpan Time { get; }

        public LocalRequest(ulong index, BDict request, TimeSpan time) {
            Response = new TaskCompletionSource<RemoteResponse>();
            Index = index;
            Request = request;
            Time = time;
        }
    }
}
