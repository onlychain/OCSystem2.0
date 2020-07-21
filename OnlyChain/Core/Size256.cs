using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public readonly struct Size256 {
#if DEBUG
        private readonly ulong _0, _1, _2, _3;
#endif
    }
}
