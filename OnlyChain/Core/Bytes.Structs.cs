using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
#pragma warning disable CA1815 // Override equals and operator equals on value types

    [StructLayout(LayoutKind.Sequential, Size = 20)]
    public readonly struct Address {
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly uint _0, _1, _2, _3, _4;
#endif
    }

    [StructLayout(LayoutKind.Sequential, Size = 20)]
    public readonly struct Hash160 {
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly uint _0, _1, _2, _3, _4;
#endif
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public readonly struct Hash256 {
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ulong _0, _1, _2, _3;
#endif
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public readonly struct U256 {
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ulong _0, _1, _2, _3;
#endif
    }

#pragma warning restore CA1815 // Override equals and operator equals on value types
}
