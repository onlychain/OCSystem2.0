using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Secp256k1.Math {
    internal readonly struct Point {
        public static readonly Point Zero = new Point(default, default, isZero: true);

        public readonly Fraction X, Y;
        public readonly bool IsZero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point(in Fraction x, in Fraction y, bool isZero = false) {
            X = x;
            Y = y;
            IsZero = isZero;
        }
    }
}
