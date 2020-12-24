using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OnlyChain.Model {
    public sealed class VoteData : AttachData {
        public uint Round { get; }
        public Bytes<Address>[] Addresses { get; }

        public VoteData(uint round, Bytes<Address>[] addresses) => (Round, Addresses) = (round, addresses);
    }
}
