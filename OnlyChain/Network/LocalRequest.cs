using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class LocalRequest {
        internal CancellationTokenSource CancellationTokenSource { get; }
        internal TaskCompletionSource<RemoteResponse> Response { get; } = new TaskCompletionSource<RemoteResponse>();
        public ulong Index { get; }
        public DateTime DateTime { get; }
        public BDict Request { get; }

        public LocalRequest(ulong index, BDict request, TimeSpan timeout, CancellationToken cancellationToken) {
            Index = index;
            DateTime = DateTime.UtcNow;
            Request = request;
            if (cancellationToken.CanBeCanceled) {
                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(timeout).Token, cancellationToken);
            } else {
                CancellationTokenSource = new CancellationTokenSource(timeout);
            }
        }
    }
}
