using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace OnlyChain.Core {
    public static class Ripemd160 {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("ripemd160", EntryPoint = "compute_hash")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:P/Invokes should not be visible", Justification = "<挂起>")]
        unsafe public extern static void ComputeHash(void* inHash, void* @out);

        [SkipLocalsInit]
        unsafe public static Bytes<Hash160> ComputeHash(Bytes<Hash256> hash) {
            Bytes<Hash160> result;
            ComputeHash(&hash, &result);
            return result;
        }
    }
}
