using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnlyChain.Core {
    public sealed class BlockDictionary : IndexedDictionary<Hash<Size256>, Block> {
        private bool saving = false;
        private readonly FileStream fileStream;
        private int prevHashCount = 0;

        public string FileName { get; }

        /// <summary>
        /// 从文件中创建<see cref="Hashes{T}"/>对象。
        /// </summary>
        /// <param name="filename">用于读取或存储所有hash的文件。</param>
        unsafe public BlockDictionary(string filename) {
            if (File.Exists(filename)) {
                using var stream = File.OpenRead(filename);
                var hashCount = Math.DivRem(stream.Length, sizeof(Hash<Size256>), out var rem);
                if (rem != 0 || hashCount > int.MaxValue) throw new ArgumentException("无效的hashes文件或已损坏", nameof(filename));
                prevHashCount = (int)hashCount;
                hashTable = new HashTable(Math.Max((int)hashCount, 1000));
                Hash<Size256> hash;
                while (hashCount-- > 0) {
                    stream.Read(new Span<byte>(&hash, sizeof(Hash<Size256>)));
                    hashTable.Add(&hash);
                }
            } else {
                hashTable = new HashTable(1000);
            }

            FileName = filename;
            fileStream = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        unsafe private void Save() {
            if (saving) throw new InvalidOperationException("另一个线程正在进行保存操作");
            saving = true;
            try {
                int currentCount = Count;
                if (prevHashCount >= currentCount) return;

                for (int i = prevHashCount; i < currentCount; i++) {
                    fileStream.Write(new ReadOnlySpan<byte>(hashTable[i], sizeof(Hash<Size256>)));
                }
                prevHashCount = currentCount;
            } finally {
                saving = false;
            }
        }

        public override int Add(Hash<Size256> hash) {
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
