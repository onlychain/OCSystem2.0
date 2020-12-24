using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBIterator : LevelDBObject<Native.leveldb_iterator_t> {
        private readonly LevelDB db;
        private readonly LevelDBReadOptions options;

        public LevelDBIterator(LevelDB db, LevelDBReadOptions options) {
            this.db = db;
            this.options = options;
            nativePointer = Native.create_iterator(db.nativePointer, options.nativePointer);
        }

        public bool IsValid => Native.iter_valid(nativePointer);

        public void Seek(ReadOnlySpan<byte> key) {
            fixed (byte* p = key) {
                Native.iter_seek(nativePointer, p, (size_t)key.Length);
            }
        }

        public void SeekToFirst() {
            Native.iter_seek_to_first(nativePointer);
        }

        public void SeekToLast() {
            Native.iter_seek_to_last(nativePointer);
        }

        public void Next() {
            Native.iter_next(nativePointer);
        }

        public void Previous() {
            Native.iter_prev(nativePointer);
        }

        public ReadOnlySpan<byte> Key {
            get {
                if (!IsValid) throw new InvalidOperationException();
                void* key = Native.iter_key(nativePointer, out var len);
                return new ReadOnlySpan<byte>(key, (int)len);
            }
        }

        public ReadOnlySpan<byte> Value {
            get {
                if (!IsValid) throw new InvalidOperationException();
                void* value = Native.iter_value(nativePointer, out var len);
                return new ReadOnlySpan<byte>(value, (int)len);
            }
        }

        protected override void UnmanagedDispose() {
            Native.iter_destroy(nativePointer);
        }
    }
}
