using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<挂起>")]
    public sealed class InvalidBlockException : Exception {
        public InvalidBlockException() : base($"无效的区块") { }
    }
}
