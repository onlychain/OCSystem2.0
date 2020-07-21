using OnlyChain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("dict Count={Count}")]
    public sealed class BDict : BObject, IDictionary<string, BObject> {
        public const byte PrefixChar = (byte)'{';

        private readonly IDictionary<string, BObject> dict;

        public BDict() => dict = new Dictionary<string, BObject>();

        public BDict(IDictionary<string, BObject> dict) => this.dict = dict;

        public BObject this[string key] {
            get => dict.TryGetValue(key, out var value) ? value : null;
            set => dict[key] = value;
        }

        public ICollection<string> Keys => dict.Keys;

        public ICollection<BObject> Values => dict.Values;

        public int Count => dict.Count;

        public bool IsReadOnly => dict.IsReadOnly;

        public void Add(string key, BObject value) {
            dict.Add(key, value);
        }

        public void Add(KeyValuePair<string, BObject> item) {
            dict.Add(item);
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
            dict.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, BObject>> GetEnumerator() {
            return dict.GetEnumerator();
        }

        public bool Remove(string key) {
            return dict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, BObject> item) {
            return dict.Remove(item);
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

        public override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            stream.WriteVarUInt((ulong)dict.Count);
            foreach (var (key, value) in dict) {
                BString.WriteNoPrefix(stream, key);
                value.Write(stream);
            }
        }

        public static implicit operator BDict(Dictionary<string, BObject> dict) => new BDict(dict);

        public static BDict Make(params (string Key, BObject Value)[] keyValuePairs) {
            var result = new Dictionary<string, BObject>();
            foreach (var (k, v) in keyValuePairs) result.Add(k, v);
            return result;
        }
    }
}
