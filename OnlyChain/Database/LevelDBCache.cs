using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBCache : LevelDBObject<Native.leveldb_cache_t> {
        public LevelDBCache(int capacity) {
            nativePointer = Native.cache_create_lru(new size_t(capacity));
        }

        protected override void UnmanagedDispose() {
            Native.cache_destroy(nativePointer);
        }
    }
}
