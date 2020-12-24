using OnlyChain.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public sealed class PeerBook : IReadOnlyDictionary<Bytes<Address>, Node> {
        private readonly Dictionary<Bytes<Address>, Node> dict;

        public int MaxCount { get; }

        public PeerBook(int maxCount) {
            MaxCount = maxCount;
            dict = new Dictionary<Bytes<Address>, Node>(maxCount);
        }

        public Node this[Bytes<Address> key] {
            get {
                lock (dict) return dict[key];
            }
        }

        public IEnumerable<Bytes<Address>> Keys {
            get {
                lock (dict) return new ReadOnlyCollection<Bytes<Address>>(dict.Keys.ToArray());
            }
        }

        public IEnumerable<Node> Values {
            get {
                lock (dict) return new ReadOnlyCollection<Node>(dict.Values.ToArray());
            }
        }

        public int Count => dict.Count;

        public bool ContainsKey(Bytes<Address> key) {
            lock (dict) {
                return dict.ContainsKey(key);
            }
        }

        public IEnumerator<KeyValuePair<Bytes<Address>, Node>> GetEnumerator() {
            KeyValuePair<Bytes<Address>, Node>[] cache;
            lock (dict) {
                cache = dict.ToArray();
            }
            return (IEnumerator<KeyValuePair<Bytes<Address>, Node>>)cache.GetEnumerator();
        }

        public bool TryGetValue(Bytes<Address> key, [MaybeNullWhen(false)] out Node value) {
            lock (dict) {
                return dict.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public void Add(Bytes<Address> address, Node node) {
            lock (dict) {
                dict.Remove(address);
                dict.Add(address, node);
                if (dict.Count > MaxCount) {
                    dict.Remove(dict.Keys.First());
                }
            }
        }
    }
}
