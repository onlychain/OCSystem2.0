using System;
using System.Diagnostics.CodeAnalysis;

namespace OnlyChain.Core {
    /// <summary>
    /// 一个固定大小的队列，当队列满了会自动移除最前面的元素。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexedQueue<T> where T : notnull {
        private readonly T[] array;
        private int index;

        public int MaxCount => array.Length;

        public int Count { get; private set; }

        public IndexedQueue(int maxCount) {
            if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
            array = new T[maxCount];
            index = 0;
            Count = 0;
        }

        /// <summary>
        /// 无论如何入队都会成功。当有旧元素被移除时，返回true，否则返回false。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public bool Enqueue(T value, [MaybeNullWhen(false)] out T oldValue) {
            bool result;
            if (Count < MaxCount) {
                array[index] = value;
                oldValue = default;
                Count++;
                result = false;
            } else {
                oldValue = array[index];
                array[index] = value;
                result = true;
            }
            index++;
            if (index >= MaxCount) index = 0;
            return result;
        }

        /// <summary>
        /// 从最后一个元素开始（包含最后一个元素，从0开始），往前索引元素。
        /// </summary>
        /// <param name="forwardIndex">向前的索引</param>
        /// <returns></returns>
        public ref T History(int forwardIndex) {
            if ((uint)forwardIndex >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(forwardIndex));
            if (forwardIndex <= index) {
                return ref array[index - forwardIndex];
            } else {
                return ref array[^(forwardIndex - index)];
            }
        }
    }
}
