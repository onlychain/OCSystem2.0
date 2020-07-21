using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class RefList<T> where T : struct {
        private T[] array;
        private readonly List<int> freeIndexes = new List<int>();

        public int Count { get; private set; }

        public ref T this[int index] => ref array[index];

        public RefList(int capacity) {
            capacity = Math.Max(capacity, 1);
            array = new T[capacity];
            Count = 0;
        }

        private void TryResize() {
            if (Count >= array.Length) {
                int newSize = array.Length <= 1024 ? array.Length * 2 : array.Length * 3 / 2;
                Array.Resize(ref array, newSize);
            }
        }

        public int Add(T item) {
            if (freeIndexes.Count > 0) {
                int i = freeIndexes[^1];
                freeIndexes.RemoveAt(freeIndexes.Count - 1);
                array[i] = item;
                return i;
            } else {
                TryResize();
                array[Count] = item;
                return Count++;
            }
        }

        public void RemoveAt(int index) {
            freeIndexes.Add(index);
        }
    }
}
