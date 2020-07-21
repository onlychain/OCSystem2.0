using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBSnapshot : LevelDBObject<Native.leveldb_snapshot_t> {
        public LevelDB Database { get; }

        public LevelDBSnapshot(LevelDB database) {
            Database = database;
            nativePointer = Native.create_snapshot(database.nativePointer);
        }

        protected override void UnmanagedDispose() {
            Native.release_snapshot(Database.nativePointer, nativePointer);
        }
    }
}
