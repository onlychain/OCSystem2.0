using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("list Count={Count}")]
    public sealed class BList : BObject, IList<BObject> {
        public const byte PrefixChar = (byte)'[';

        private readonly List<BObject> objects;

        public BList(List<BObject> objects) => this.objects = objects;

        public BObject this[int index] { get => objects[index]; set => objects[index] = value; }

        public int Count => objects.Count;

        bool ICollection<BObject>.IsReadOnly => false;

        public void Add(BObject item) {
            objects.Add(item);
        }

        public void Clear() {
            objects.Clear();
        }

        public bool Contains(BObject item) {
            return objects.Contains(item);
        }

        public void CopyTo(BObject[] array, int arrayIndex) {
            objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<BObject> GetEnumerator() {
            return objects.GetEnumerator();
        }

        public int IndexOf(BObject item) {
            return objects.IndexOf(item);
        }

        public void Insert(int index, BObject item) {
            objects.Insert(index, item);
        }

        public bool Remove(BObject item) {
            return objects.Remove(item);
        }

        public void RemoveAt(int index) {
            objects.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return objects.GetEnumerator();
        }

        public override void Write(ref BWriteArgs args) {
            args.Stream.WriteByte(PrefixChar);
            args.Stream.WriteVarUInt((ulong)objects.Count);
            foreach (var obj in objects) {
                obj.Write(ref args);
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append('[');

            bool first = true;
            foreach (var v in this) {
                if (first) {
                    sb.Append(',');
                    first = false;
                }
                sb.Append(v);
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
