using OnlyChain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("dict Count={Count}")]
    public sealed class BDict : BObject, IDictionary<string, BObject> {
        public const byte PrefixChar = (byte)'{';

        public static readonly IComparer<string> SortedKeyComparer = Comparer<string>.Create((a, b) => {
            int cmp = a.Length.CompareTo(b.Length);
            if (cmp != 0) return cmp;
            return a.AsSpan().SequenceCompareTo(b);
        });

        private readonly Dictionary<string, BObject> dict;

        public BDict() => dict = new Dictionary<string, BObject>();

        public BDict(Dictionary<string, BObject> dict) => this.dict = dict;

        public BObject this[string key] {
            get => dict.TryGetValue(key, out var value) ? value : null;
            set => dict[key] = value;
        }

        public ICollection<string> Keys => dict.Keys;

        public ICollection<BObject> Values => dict.Values;

        public int Count => dict.Count;

        bool ICollection<KeyValuePair<string, BObject>>.IsReadOnly => false;

        public void Add(string key, BObject value) {
            dict.Add(key, value);
        }

        public void Add(KeyValuePair<string, BObject> item) {
            dict.Add(item.Key, item.Value);
        }

        public void Clear() {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<string, BObject> item) {
            return dict.Contains(item);
        }

        public bool ContainsKey(string key) {
            return dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, BObject>[] array, int arrayIndex) {
            ((IDictionary<string, BObject>)dict).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, BObject>> GetEnumerator() {
            return dict.GetEnumerator();
        }

        public bool Remove(string key) {
            return dict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, BObject> item) {
            return ((IDictionary<string, BObject>)dict).Remove(item);
        }

        public bool TryGetValue(string key, out BObject value) {
            return dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return dict.GetEnumerator();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (dict.TryGetValue(binder.Name, out var bobj)) {
                result = bobj;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            if (value is BObject obj) {
                dict[binder.Name] = obj;
                return true;
            }
            return base.TrySetMember(binder, value);
        }

        public override void Write(ref BWriteArgs args) {
            args.Stream.WriteByte(PrefixChar);
            args.Stream.WriteVarUInt((ulong)this.dict.Count);

            IDictionary<string, BObject> dict;
            if (args.SortedKey) {
                dict = new SortedDictionary<string, BObject>(this.dict, SortedKeyComparer);
            } else {
                dict = this.dict;
            }

            foreach (var (key, value) in dict) {
                BString.WriteNoPrefix(args.Stream, key);
                value.Write(ref args);
            }
        }

        public BDict Clone() => new Dictionary<string, BObject>(dict);

        public static implicit operator BDict(Dictionary<string, BObject> dict) => new BDict(dict);

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append('{');

            bool first = true;
            foreach (var (k, v) in this) {
                if (first) {
                    sb.Append(',');
                    first = false;
                }
                sb.Append('"').Append(k).Append('"');
                sb.Append(':');
                sb.Append(v);
            }

            sb.Append('}');
            return sb.ToString();
        }

        public static BDict Make(params (string Key, BObject Value)[] keyValuePairs) {
            var result = new Dictionary<string, BObject>();
            foreach (var (k, v) in keyValuePairs) result.Add(k, v);
            return result;
        }
    }
}
