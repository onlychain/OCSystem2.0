using System;
using System.Diagnostics;

namespace OnlyChain.Database {
    internal static class LevelDBErrorEx {
        [DebuggerStepThrough, DebuggerHidden]
        unsafe public static void FreeTryThrow(this IntPtr error) {
            if (error != IntPtr.Zero) {
                try {
                    throw new LevelDBException(error);
                } finally {
                    Native.free((void*)error);
                }
            }
        }
    }
}
