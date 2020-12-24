using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class BlockChannel : Channel<Block> {
        
        
        private readonly Channel<Block> channel = Channel.CreateUnbounded<Block>(new UnboundedChannelOptions { SingleReader = true });


    }
}
