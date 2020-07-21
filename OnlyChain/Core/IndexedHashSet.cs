using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Linq;

namespace OnlyChain.Core {
    /// <summary>
    /// 无锁哈希表。查询Hash对应的索引，或根据索引查询对应的Hash。
    /// <para>在单线程写、多线程读的情况下是线程安全的。</para>
    /// </summary>
    /// <typeparam name="T">Hash类型</typeparam>
    public class IndexedHashSet<T> : IDisposable, IReadOnlyList<T>, IReadOnlyDictionary<T, int> where T : unmanaged {
        unsafe static IndexedHashSet() {
            if (sizeof(T) % 4 != 0 || typeof(T) == typeof(int)) throw new TypeLoadException("不支持的类型参数");
        }

        const int NextMask = unchecked((int)0x80000000);
        const int IndexMask = ~NextMask;

        [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<挂起>")]
        unsafe protected struct HashTable : IEnumerable<T>, IDisposable {
            sealed class NativeArray : IDisposable {
                public readonly T* List;
                public readonly int* Buckets;
                public readonly int ListLength, BucketsLength;

                public NativeArray(int listLength, int bucketsLength) {
                    List = (T*)Marshal.AllocHGlobal((IntPtr)((long)sizeof(T) * listLength + (long)sizeof(int) * bucketsLength));
                    Buckets = (int*)(List + listLength);
                    ListLength = listLength;
                    BucketsLength = bucketsLength;
                }

                public Span<T> ListSpan => new Span<T>(List, ListLength);
                public ReadOnlySpan<T> ListReadOnlySpan => new ReadOnlySpan<T>(List, ListLength);
                public Span<int> BucketsSpan => new Span<int>(Buckets, BucketsLength);
                public ReadOnlySpan<int> BucketsReadOnlySpan => new ReadOnlySpan<int>(Buckets, BucketsLength);

                public NativeArray ExpandList() {
                    var newArray = new NativeArray(ListLength * 3 / 2, BucketsLength);
                    ListReadOnlySpan.CopyTo(newArray.ListSpan);
                    BucketsReadOnlySpan.CopyTo(newArray.BucketsSpan);
                    return newArray;
                }

                public NativeArray ExpandBuckets() {
                    var newArray = new NativeArray(ListLength, BucketsLength * 2);
                    ListReadOnlySpan.CopyTo(newArray.ListSpan);
                    newArray.BucketsSpan.Fill(-1);
                    return newArray;
                }

                [SuppressMessage("Style", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
                void Dispose(bool disposing) {
                    Marshal.FreeHGlobal((IntPtr)List);
                }

                ~NativeArray() {
                    Dispose(false);
                }

                public void Dispose() {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
            }

            const float LoadFactor = 0.72f;

            private NativeArray array;
            private int count;
            private int loadSize;

            public readonly int Count => count;

            public HashTable(int capacity) {
                int capacityLevel = 32 - BitOperations.LeadingZeroCount((uint)capacity);
                array = new NativeArray(capacity, 1 << capacityLevel);
                array.BucketsSpan.Fill(-1);
                count = 0;
                loadSize = (int)(array.BucketsLength * LoadFactor);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetHashCode(T* key) => *(int*)key;

            private static bool KeyEquals(T* k1, T* k2) {
                for (int i = 0; i < sizeof(T) / sizeof(long); i++) {
                    if (((long*)k1)[i] != ((long*)k2)[i]) return false;
                }
                if (sizeof(T) % sizeof(long) != 0) {
                    if (((int*)k1)[sizeof(T) / sizeof(int) - 1] != ((int*)k2)[sizeof(T) / sizeof(int) - 1]) return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetIncrement(int hashCode) => hashCode | 1; // 和2^N互质

            private static int Get(NativeArray array, T* key) {
                int hashCode = GetHashCode(key);
                int indexMask = array.BucketsLength - 1;
                int bucketIndex = hashCode & indexMask;

                while (true) {
                    int index = array.Buckets[bucketIndex];
                    if (index == -1) return -1;
                    T* other = array.List + (index & IndexMask);
                    if (hashCode == GetHashCode(other)) {
                        if (KeyEquals(key, other)) return index & IndexMask;
                        if ((index & NextMask) == 0) return -1;
                    }
                    bucketIndex = (bucketIndex + GetIncrement(hashCode)) & indexMask;
                }
            }

            private static void Put(NativeArray array, T* key, int value) {
                int hashCode = GetHashCode(key);
                int indexMask = array.BucketsLength - 1;
                int bucketIndex = hashCode & indexMask;

                while (true) {
                    int index = array.Buckets[bucketIndex];
                    if (index == -1) {
                        array.Buckets[bucketIndex] = value;
                        return;
                    }
                    key = array.List + (index & IndexMask);
                    if (hashCode == GetHashCode(key)) {
                        array.Buckets[bucketIndex] |= NextMask;
                    }
                    bucketIndex = (bucketIndex + GetIncrement(hashCode)) & indexMask;
                }
            }

            public readonly int GetIndex(T* key) => Get(array, key);

            public int Add(T* key) {
                var array = this.array;
                if (count >= loadSize) {
                    array = array.ExpandBuckets();
                    for (int i = 0; i < count; i++) {
                        Put(array, array.List + i, i);
                    }
                    loadSize = (int)(array.BucketsLength * LoadFactor);
                }
                if (count >= array.ListLength) {
                    array = array.ExpandList();
                }
                array.List[count] = *key;
                Put(array, key, count);
                this.array = array;
                return count++;
            }

            public readonly T* this[int index] => array.List + index;

            public readonly void Dispose() {
                array.Dispose();
            }

            public readonly IReadOnlyList<T> GetReadOnlyList() => new Enumerator(this);

            public readonly IEnumerator<T> GetEnumerator() => new Enumerator(this);

            readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


            sealed class Enumerator : IReadOnlyList<T>, IEnumerator<T> {
                private readonly NativeArray array;
                private readonly int count;
                private int index = -1;

                public Enumerator(in HashTable hashTable) {
                    count = hashTable.count;
                    array = hashTable.array;
                }

                public T this[int index] => array.List[index];

                public T Current => array.List[index];

                public int Count => count;

                object IEnumerator.Current => Current;

                public void Dispose() {

                }

                public IEnumerator<T> GetEnumerator() => this;

                public bool MoveNext() {
                    int nextIndex = index + 1;
                    if (nextIndex < count) {
                        index = nextIndex;
                        return true;
                    }
                    return false;
                }

                public void Reset() {
                    throw new NotSupportedException();
                }

                IEnumerator IEnumerable.GetEnumerator() => this;
            }
        }



        protected HashTable hashTable;

        public int Count => hashTable.Count;

        public IReadOnlyList<T> Keys => hashTable.GetReadOnlyList();

        IEnumerable<T> IReadOnlyDictionary<T, int>.Keys => Keys;

        public IEnumerable<int> Values => Enumerable.Range(0, Count);

        unsafe int IReadOnlyDictionary<T, int>.this[T key] => hashTable.GetIndex(&key);

        /// <summary>
        /// 查询索引对应的hash。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        unsafe public T this[int index] => (uint)index < (uint)hashTable.Count ? *hashTable[index] : throw new ArgumentOutOfRangeException(nameof(index));

        protected IndexedHashSet() { }

        public IndexedHashSet(int capacity) {
            hashTable = new HashTable(capacity);
        }

        /// <summary>
        /// 此方法不检查添加的hash是否已存在。
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        unsafe public virtual int Add(T hash) => hashTable.Add(&hash);

        /// <summary>
        /// 查询hash对应的索引。
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        unsafe public int GetIndex(T hash) => hashTable.GetIndex(&hash);

        unsafe public int GetOrCreateIndex(T hash) {
            if (hashTable.GetIndex(&hash) is int index && index >= 0) {
                return index;
            }
            return Add(hash);
        }

        #region IDisposable Support
        protected bool isDisposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    hashTable.Dispose();
                }
                isDisposed = true;
            }
        }

        ~IndexedHashSet() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public IEnumerator<T> GetEnumerator() => hashTable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        unsafe public bool ContainsKey(T key) => hashTable.GetIndex(&key) >= 0;

        unsafe public bool TryGetValue(T key, [MaybeNullWhen(false)] out int value) {
            int index = hashTable.GetIndex(&key);
            if (index >= 0) {
                value = index;
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator()
            => hashTable.Select((key, i) => new KeyValuePair<T, int>(key, i)).GetEnumerator();
    }
}
