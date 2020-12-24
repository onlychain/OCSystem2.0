using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public static class MathTools {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckAdd(ulong a, ulong b, out ulong result) {
            ulong c = unchecked(a + b);
            result = c;
            return c >= a && c >= b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSub(ulong a, ulong b, out ulong result) {
            ulong c = unchecked(a - b);
            result = c;
            return a >= b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckMul(ulong a, ulong b, out ulong result) {
            return Math.BigMul(a, b, out result) == 0;
        }
    }
}
