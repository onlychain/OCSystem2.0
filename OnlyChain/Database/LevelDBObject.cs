using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    internal abstract class LevelDBObject<T> : IDisposable where T : unmanaged {
        unsafe protected internal T* nativePointer;

        #region IDisposable Support

        protected abstract void UnmanagedDispose();

        protected virtual void ManagedDispose() { }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        unsafe private void Dispose(bool disposing) {
            if (nativePointer != null) {
                if (disposing) {
                    ManagedDispose();
                }
                UnmanagedDispose();
                nativePointer = null;
            }
        }
#pragma warning restore CA1063 // Implement IDisposable Correctly

        ~LevelDBObject() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
