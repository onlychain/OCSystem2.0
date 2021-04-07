using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public class RequestTimeoutException : TimeoutException {
        public LocalRequest RequestSource { get; }

        public RequestTimeoutException(LocalRequest requestSource) => RequestSource = requestSource;
    }
}
