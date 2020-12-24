using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network.Objects {
    public ref struct BWriteArgs {
        public Stream Stream;
        public bool SortedKey;
    }
}
