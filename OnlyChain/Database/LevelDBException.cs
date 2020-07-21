using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Database {
#pragma warning disable CA1032 // Implement standard exception constructors
    internal class LevelDBException : Exception {
        public LevelDBException(IntPtr error) : base(Marshal.PtrToStringAnsi(error)) { }
    }
#pragma warning restore CA1032 // Implement standard exception constructors
}
