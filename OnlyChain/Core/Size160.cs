using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    [StructLayout(LayoutKind.Sequential, Size = 20)]
    public readonly struct Size160 {
#if DEBUG
        private readonly uint _0, _1, _2, _3, _4;
#endif
    }
}
