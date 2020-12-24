using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public class DictionaryCounter<TKey> : IDictionary<TKey, int> {
        private readonly Dictionary<TKey, int> counter = new();

        public int this[TKey key] {
            get => counter.TryGetValue(key, out int count) ? count : 0;
            set => counter[key] = value;
        }

        public ICollection<TKey> Keys => ((IDictionary<TKey, int>)counter).Keys;

        public ICollection<int> Values => ((IDictionary<TKey, int>)counter).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, int>>)counter).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, int>>)counter).IsReadOnly;

        public void Add(TKey key, int value) {
            ((IDictionary<TKey, int>)counter).Add(key, value);
        }

        public void Add(KeyValuePair<TKey, int> item) {
            ((ICollection<KeyValuePair<TKey, int>>)counter).Add(item);
        }

        public void Clear() {
            ((ICollection<KeyValuePair<TKey, int>>)counter).Clear();
        }

        public bool Contains(KeyValuePair<TKey, int> item) {
            return ((ICollection<KeyValuePair<TKey, int>>)counter).Contains(item);
        }

        public bool ContainsKey(TKey key) {
            return ((IDictionary<TKey, int>)counter).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, int>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<TKey, int>>)counter).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<TKey, int>>)counter).GetEnumerator();
        }

        public bool Remove(TKey key) {
            return ((IDictionary<TKey, int>)counter).Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, int> item) {
            return ((ICollection<KeyValuePair<TKey, int>>)counter).Remove(item);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out int value) {
            return ((IDictionary<TKey, int>)counter).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)counter).GetEnumerator();
        }
    }
}
