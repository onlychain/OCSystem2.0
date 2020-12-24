#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Database {
    [System.Diagnostics.DebuggerDisplay("leveldb: {Name}")]
    unsafe internal sealed class LevelDB : LevelDBObject<Native.leveldb_t> {
        private readonly LevelDBOptions options;

        public string Name { get; }

        public LevelDB(string name) : this(name, LevelDBOptions.Default) { }

        public LevelDB(string name, LevelDBOptions options) {
            Name = name;
            this.options = options;
            nativePointer = Native.open(options.nativePointer, name, out var err);
            err.FreeTryThrow();
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, bool sync = false) {
            Put(key, value, sync ? LevelDBWriteOptions.DefaultSync : LevelDBWriteOptions.Default);
        }

        public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, LevelDBWriteOptions options) {
            fixed (byte* pKey = key)
            fixed (byte* pValue = value) {
                Native.put(nativePointer, options.nativePointer, pKey, (size_t)key.Length, pValue, (size_t)value.Length, out IntPtr err);
                err.FreeTryThrow();
            }
        }

        public bool Get(ReadOnlySpan<byte> key, Span<byte> value, LevelDBReadOptions? options = null) {
            options ??= LevelDBReadOptions.FillCacheOptions;
            fixed (byte* pKey = key)
            fixed (byte* pValue = value) {
                IntPtr tmpValuePointer = (IntPtr)pValue;
                int tmpValueLength = value.Length;
                var result = Native.get(nativePointer, options.nativePointer, pKey, (size_t)key.Length, (val, valLength) => {
                    if ((ulong)valLength > int.MaxValue) throw new InvalidOperationException("取出来的值太大");
                    new ReadOnlySpan<byte>(val, (int)valLength).CopyTo(new Span<byte>(tmpValuePointer.ToPointer(), tmpValueLength));
                }, out IntPtr err);
                err.FreeTryThrow();
                return result;
            }
        }

        public bool Get<T>(ReadOnlySpan<byte> key, out T value, LevelDBReadOptions? options = null) where T : unmanaged {
            fixed (T* p = &value) {
                return Get(key, new Span<byte>(p, sizeof(T)), options);
            }
        }


        public T[]? Get<T>(ReadOnlySpan<byte> key, LevelDBReadOptions? options = null) where T : unmanaged {
            options ??= LevelDBReadOptions.FillCacheOptions;
            fixed (byte* pKey = key) {
                T[]? result = null;
                Native.get(nativePointer, options.nativePointer, pKey, (size_t)key.Length, (val, valLength) => {
                    if ((ulong)valLength > int.MaxValue) throw new InvalidOperationException("取出来的值太大");
                    if ((int)valLength % sizeof(T) != 0) throw new InvalidOperationException("值未对齐");
                    result = new T[(int)valLength / sizeof(T)];
                    new ReadOnlySpan<T>(val, (int)valLength / sizeof(T)).CopyTo(result);
                }, out IntPtr err);
                err.FreeTryThrow();
                return result;
            }
        }

        public byte[]? Get(ReadOnlySpan<byte> key, LevelDBReadOptions? options = null) {
            return Get<byte>(key, options);
        }

        public void Delete(ReadOnlySpan<byte> key, bool sync = false) {
            Delete(key, sync ? LevelDBWriteOptions.DefaultSync : LevelDBWriteOptions.Default);
        }

        public void Delete(ReadOnlySpan<byte> key, LevelDBWriteOptions options) {
            fixed (byte* pKey = key) {
                Native.delete(nativePointer, options.nativePointer, pKey, (size_t)key.Length, out IntPtr err);
                err.FreeTryThrow();
            }
        }

        public void Write(LevelDBWriteBatch writeBatch, bool sync = false) {
            Write(writeBatch, sync ? LevelDBWriteOptions.DefaultSync : LevelDBWriteOptions.Default);
        }

        public void Write(LevelDBWriteBatch writeBatch, LevelDBWriteOptions options) {
            Native.write(nativePointer, options.nativePointer, writeBatch.nativePointer, out IntPtr err);
            err.FreeTryThrow();
        }

        public void Repair() {
            Native.repair_db(options.nativePointer, Name, out IntPtr err);
            err.FreeTryThrow();
        }

        protected override void UnmanagedDispose() {
            Native.close(nativePointer);
        }
    }
}
