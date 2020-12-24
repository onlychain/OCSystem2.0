#nullable enable

using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class BroadcastEventArgs : EventArgs {
        public readonly Node Sender;
        public readonly int TTL;
        public readonly string Command;
        public readonly BDict Message;
        public Task? Task { get; set; }
        public bool IsCancelForward { get; private set; } = false;


        public BroadcastEventArgs(Node sender, int ttl, string cmd, BDict message) {
            Sender = sender;
            TTL = ttl;
            Command = cmd;
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
