using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using OnlyChain.Core;

namespace OnlyChain.Database {
    /// <summary>
    /// 存储在硬盘中的无锁哈希表。
    /// </summary>
    /// <typeparam name="T">Hash类型</typeparam>
    public sealed class Hashes<T> : IndexedHashSet<T> where T : unmanaged {
        private int saving = 0;
        private readonly FileStream fileStream;
        private int prevHashCount = 0;

        public string FileName { get; }

        /// <summary>
        /// 从文件中创建<see cref="Hashes{T}"/>对象。
        /// </summary>
        /// <param name="filename">用于读取或存储所有hash的文件。</param>
        unsafe public Hashes(string filename) {
            if (File.Exists(filename)) {
                using var stream = File.OpenRead(filename);
                var hashCount = Math.DivRem(stream.Length, sizeof(T), out var rem);
                if (rem != 0 || hashCount > int.MaxValue) throw new ArgumentException("无效的hashes文件或已损坏", nameof(filename));
                prevHashCount = (int)hashCount;
                hashTable = new HashTable(Math.Max((int)hashCount, 1000));
                T hash;
                while (hashCount-- > 0) {
                    stream.Read(new Span<byte>(&hash, sizeof(T)));
                    hashTable.Add(&hash);
                }
            } else {
                hashTable = new HashTable(1000);
            }

            FileName = filename;
            fileStream = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        unsafe public void Save() {
            if (Interlocked.CompareExchange(ref saving, 1, 0) is 1) throw new InvalidOperationException("另一个线程正在进行保存操作");
            try {
                int currentCount = Count;
                if (prevHashCount >= currentCount) return;

                for (int i = prevHashCount; i < currentCount; i++) {
                    fileStream.Write(new ReadOnlySpan<byte>(hashTable[i], sizeof(T)));
                }
                prevHashCount = currentCount;
            } finally {
                saving = 0;
            }
        }

        public override int Add(T hash) {
            int index = base.Add(hash);
            Save();
            return index;
        }

        protected override void Dispose(bool disposing) {
            if (!isDisposed) {
                Save();
                if (disposing) {
                    fileStream.Close();
                }
            }
            base.Dispose(disposing);
        }
    }
}
