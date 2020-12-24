using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Core {
    public static class RefStructEx {
        unsafe public static bool IsNull(in this UserState userStatus) {
            return Unsafe.IsNullRef(ref Unsafe.AsRef(in userStatus));
        }

        unsafe public static bool IsNull<T>(in this Bytes<T> bytes) where T : unmanaged {
            return Unsafe.IsNullRef(ref Unsafe.AsRef(in bytes));
        }
    }
}
