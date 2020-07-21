using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBFilterPolicy : LevelDBObject<Native.leveldb_filterpolicy_t> {
        public LevelDBFilterPolicy(int bitsPreKey) {
            nativePointer = Native.filterpolicy_create_bloom(bitsPreKey);
        }

        protected override void UnmanagedDispose() {
            Native.filterpolicy_destroy(nativePointer);
        }
    }
}
