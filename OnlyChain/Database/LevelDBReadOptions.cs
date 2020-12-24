#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBReadOptions : LevelDBObject<Native.leveldb_readoptions_t> {
        public readonly static LevelDBReadOptions Default = new LevelDBReadOptions();
        public readonly static LevelDBReadOptions FillCacheOptions = new LevelDBReadOptions { FillCache = true };

#pragma warning disable IDE0052 // 删除未读的私有成员
#pragma warning disable CA2213 // Disposable fields should be disposed
        private LevelDBSnapshot? snapshot = null;
#pragma warning restore CA2213 // Disposable fields should be disposed
#pragma warning restore IDE0052 // 删除未读的私有成员

        public LevelDBReadOptions() {
            nativePointer = Native.readoptions_create();
        }

        public bool VerifyChecksums {
            set => Native.readoptions_set_verify_checksums(nativePointer, value);
        }

        public bool FillCache {
            set => Native.readoptions_set_fill_cache(nativePointer, value);
        }

        public LevelDBSnapshot? Snapshot {
            set {
                snapshot = value;
                if (value is { }) {
                    Native.readoptions_set_snapshot(nativePointer, value.nativePointer);
                } else {
                    Native.readoptions_set_snapshot(nativePointer, null);
                }
            }
        }

        protected override void ManagedDispose() {
            snapshot?.Dispose();
            base.ManagedDispose();
        }

        protected override void UnmanagedDispose() {
            Native.readoptions_destroy(nativePointer);
        }
    }
}
