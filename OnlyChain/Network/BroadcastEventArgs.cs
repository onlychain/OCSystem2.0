#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class BroadcastEventArgs : EventArgs {
        public readonly Node Sender;
        public readonly int TTL;
        public readonly byte[] Message;
        public Task? Task { get; set; }
        public bool IsCancelForward { get; private set; } = false;


        public BroadcastEventArgs(Node sender, int ttl, byte[] message) {
            Sender = sender;
            TTL = ttl;
            Message = message;
        }

        /// <summary>
        /// 取消广播转发。
        /// </summary>
        public void CancelForward() {
            IsCancelForward = true;
        }
    }
}
