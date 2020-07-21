#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Database {
    unsafe internal sealed class LevelDBOptions : LevelDBObject<Native.leveldb_options_t> {
        public static readonly LevelDBOptions Default = new LevelDBOptions { CreateIfMissing = true };

#pragma warning disable IDE0052 // 删除未读的私有成员
#pragma warning disable CA2213 // Disposable fields should be disposed
        private LevelDBCache? cache = null;
        private LevelDBComparator? comparator = null;
        private LevelDBFilterPolicy? filterPolicy = null;
#pragma warning restore CA2213 // Disposable fields should be disposed
#pragma warning restore IDE0052 // 删除未读的私有成员

        public LevelDBOptions() {
            nativePointer = Native.options_create();
        }

        public bool CreateIfMissing {
            set => Native.options_set_create_if_missing(nativePointer, value);
        }

        public bool ErrorIfExists {
            set => Native.options_set_error_if_exists(nativePointer, value);
        }

        public bool Compression {
            set => Native.options_set_compression(nativePointer, value);
        }

        public int BlockSize {
            set => Native.options_set_block_size(nativePointer, new size_t(value));
        }

        public int BlockRestartInterval {
            set => Native.options_set_block_restart_interval(nativePointer, value);
        }

        public int WriteBufferSize {
            set => Native.options_set_write_buffer_size(nativePointer, new size_t(value));
        }

        public int MaxOpenFiles {
            set => Native.options_set_max_open_files(nativePointer, value);
        }

        public long MaxFileSize {
            set {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                Native.options_set_max_file_size(nativePointer, (ulong)value);
            }
        }

        public bool ParanoidChecks {
            set => Native.options_set_paranoid_checks(nativePointer, value);
        }

        public LevelDBCache? Cache {
            set {
                cache = value;
                if (value is { }) {
                    Native.options_set_cache(nativePointer, value.nativePointer);
                } else {
                    Native.options_set_cache(nativePointer, null);
                }
            }
        }

        public LevelDBComparator? Comparator {
            set {
                comparator = value;
                if (value is { }) {
                    Native.options_set_comparator(nativePointer, value.nativePointer);
                } else {
                    Native.options_set_comparator(nativePointer, null);
                }
            }
        }

        public LevelDBFilterPolicy? FilterPolicy {
            set {
                filterPolicy = value;
                if (value is { }) {
                    Native.options_set_filter_policy(nativePointer, value.nativePointer);
                } else {
                    Native.options_set_filter_policy(nativePointer, null);
                }
            }
        }

        protected override void ManagedDispose() {
            cache?.Dispose();
            comparator?.Dispose();
            filterPolicy?.Dispose();
            base.ManagedDispose();
        }

        protected override void UnmanagedDispose() {
            Native.options_destroy(nativePointer);
        }
    }
}
