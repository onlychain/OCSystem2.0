using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OnlyChain.Core {
    public class IndexedDictionary<TKey, TValue> : IndexedHashSet<TKey>, IReadOnlyDictionary<TKey, TValue> where TKey : unmanaged {
        protected List<TValue> values;

        protected IndexedDictionary() { }

        public IndexedDictionary(int capacity) : base(capacity) {
            values = new List<TValue>(capacity);
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        public new IReadOnlyList<TValue> Values => values;

        public TValue this[TKey key] => TryGetValue(key, out TValue value) ? value : throw new KeyNotFoundException();

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => values;

        unsafe public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
            int index = hashTable.GetIndex(&key);
            if (index >= 0) {
                value = values[index];
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            int count = Count;
            for (int i = 0; i < count; i++) {
                yield return new KeyValuePair<TKey, TValue>(this[i], values[i]);
            }
        }

        public override int Add(TKey key) {
            values.Add(default);
            return base.Add(key);
        }

        public int Add(TKey key, TValue value) {
            values.Add(value);
            return base.Add(key);
        }

        unsafe public int Set(TKey key, TValue value) {
            int index = hashTable.GetIndex(&key);
            if (index < 0) return Add(key, value);
            values[index] = value;
            return index;
        }

        unsafe public int GetOrCreateIndex(TKey key, TValue value) {
            int index = hashTable.GetIndex(&key);
            if (index >= 0) return index;
            return Add(key, value);
        }
    }
}
