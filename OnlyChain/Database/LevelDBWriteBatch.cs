using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBWriteBatch : LevelDBObject<Native.leveldb_writebatch_t> {
        public LevelDBWriteBatch() {
            nativePointer = Native.writebatch_create();
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) {
            fixed (byte* pKey = key)
            fixed (byte* pValue = value)
                Native.writebatch_put(nativePointer, pKey, (size_t)key.Length, pValue, (size_t)value.Length);
        }

        public void Delete(ReadOnlySpan<byte> key) {
            fixed (byte* pKey = key)
                Native.writebatch_delete(nativePointer, pKey, (size_t)key.Length);
        }

        public void Append(LevelDBWriteBatch other) {
            Native.writebatch_append(nativePointer, other.nativePointer);
        }

        public void Clear() {
            Native.writebatch_clear(nativePointer);
        }

        protected override void UnmanagedDispose() {
            Native.writebatch_destroy(nativePointer);
        }
    }
}
