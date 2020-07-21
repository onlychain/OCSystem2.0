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

        unsafe public static Hash<Size160> ComputeHash(Hash<Size256> hash) {
            Hash<Size160> result;
            ComputeHash(&hash, &result);
            return result;
        }
    }
}
