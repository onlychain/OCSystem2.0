using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace OnlyChain.Core {
    public sealed class LimitedTimeDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {
        private readonly object @lock = new object();
        private readonly Stopwatch timer;
        private readonly TimeSpan timeout;
        private readonly ConcurrentDictionary<TKey, (TValue Value, TimeSpan TimeSpan)> dict = new();

        public LimitedTimeDictionary(TimeSpan timeout) {
            this.timeout = timeout;
            timer = Stopwatch.StartNew();
        }

        public TValue this[TKey key] {
            get {
                if (dict.TryGetValue(key, out var pair)) {
                    return pair.Value;
                }
                throw new KeyNotFoundException();
            }
        }

        public IEnumerable<TKey> Keys => dict.Keys;

        public IEnumerable<TValue> Values => dict.Values.Select(p => p.Value);

        public int Count => dict.Count;

        public bool ContainsKey(TKey key) {
            return dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            foreach (var (k, v) in dict) {
                yield return new(k, v.Value);
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
            if (dict.TryGetValue(key, out var p)) {
                value = p.Value;
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TKey key, TValue value) {
            if (Monitor.TryEnter(@lock)) {
                try {
                    var removeKeys = new List<TKey>();
                    foreach (var (k, v) in dict) {
                        if (v.TimeSpan + timeout >= timer.Elapsed) break;
                        removeKeys.Add(k);
                    }
                    foreach (var k in removeKeys) dict.TryRemove(k, out _);
                } finally {
                    Monitor.Exit(@lock);
                }
            }

            dict.TryAdd(key, (value, timer.Elapsed));
        }

        public void Remove(TKey key) {
            dict.Remove(key, out _);
        }
    }
}
