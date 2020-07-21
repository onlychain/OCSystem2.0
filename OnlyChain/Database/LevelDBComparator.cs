using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Database {
    unsafe internal class LevelDBComparator : LevelDBObject<Native.leveldb_comparator_t> {
        public delegate int ComparatorDelegate(ReadOnlySpan<byte> key1, ReadOnlySpan<byte> key2);

        private static readonly Native.leveldb_comparator_create_destructor emptyDestructor = delegate { };

        private readonly IntPtr namePointer;
        private readonly Native.leveldb_comparator_create_name comparatorName;
        private readonly Native.leveldb_comparator_create_compare comparator;

        public readonly string Name;
        public readonly ComparatorDelegate Comparator;

        public LevelDBComparator(string name, ComparatorDelegate comparator) {
            Name = name;
            Comparator = comparator;
            namePointer = Marshal.StringToHGlobalAnsi(name);
            comparatorName = delegate { return (void*)namePointer; };
            this.comparator = (_, k1, klen1, k2, klen2) => {
                return Comparator(new ReadOnlySpan<byte>(k1, (int)klen1), new ReadOnlySpan<byte>(k2, (int)klen2));
            };
            nativePointer = Native.comparator_create(null, emptyDestructor, this.comparator, comparatorName);
        }

        protected override void UnmanagedDispose() {
            Marshal.FreeHGlobal(namePointer);
            Native.comparator_destroy(nativePointer);
        }
    }
}
