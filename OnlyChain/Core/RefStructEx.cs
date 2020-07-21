using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Core {
    public static class RefStructEx {
        unsafe public static bool IsNull(in this UserState userStatus) {
            return Unsafe.AsPointer(ref Unsafe.AsRef(userStatus)) == null;
        }
    }
}
