using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace OnlyChain.Core {
    public class DiskList<T> : IDisposable, IList<T> where T : unmanaged {
        protected readonly List<T> list;
        protected readonly FileStream stream;

        unsafe public DiskList(string filename) {
            stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            long count = Math.DivRem(stream.Length, sizeof(T), out long rem);
            if (rem != 0 || count >= int.MaxValue) throw new ArgumentException($"无效的{nameof(DiskList<T>)}文件：{filename}", nameof(filename));

            stream.Seek(0, SeekOrigin.Begin);
            list = new List<T>(unchecked((int)count));
            T item;
            for (int i = 0; i < unchecked((int)count); i++) {
                stream.Read(new Span<byte>(&item, sizeof(T)));
                list.Add(item);
            }
        }


        unsafe public int Count => list.Count;

        public bool IsReadOnly => false;

        unsafe public T this[int index] {
            get => list[index];
            set {
                list[index] = value;
                stream.Position = index * sizeof(T);
                stream.Write(new ReadOnlySpan<byte>(&value, sizeof(T)));
            }
        }

        public int IndexOf(T item) {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        unsafe public void Add(T item) {
            list.Add(item);
            stream.Seek(0, SeekOrigin.End);
            stream.Write(new ReadOnlySpan<byte>(&item, sizeof(T)));
        }

        public void Clear() {
            throw new NotSupportedException();
        }

        public bool Contains(T item) {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        public bool Remove(T item) {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        #region IDisposable Support
        protected bool isDisposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    stream.Dispose();
                }
                isDisposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }
}
