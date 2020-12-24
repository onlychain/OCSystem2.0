using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OnlyChain.Secp256k1.Math {
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Point {
        public readonly U256P X, Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point(in U256P x, in U256P y) {
            X = x;
            Y = y;
        }
    }
}
