using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBWriteOptions : LevelDBObject<Native.leveldb_writeoptions_t> {
        public readonly static LevelDBWriteOptions Default = new LevelDBWriteOptions();
        public readonly static LevelDBWriteOptions DefaultSync = new LevelDBWriteOptions() { Sync = true };

        public LevelDBWriteOptions() {
            nativePointer = Native.writeoptions_create();
        }

        public bool Sync {
            set => Native.writeoptions_set_sync(nativePointer, value);
        }

        protected override void UnmanagedDispose() {
            Native.writeoptions_destroy(nativePointer);
        }
    }
}
