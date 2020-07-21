using System;
using System.IO;
using System.Collections.Generic;
using OnlyChain.Secp256k1;
using System.Threading;

namespace OnlyChain.Core {
    public sealed class UserDictionary : IndexedDictionary<Address, PublicKeyStruct> {
        unsafe static readonly int StructSize = sizeof(Address) + sizeof(PublicKeyStruct);

        private int saving = 0;
        private readonly FileStream fileStream;
        private int prevHashCount = 0;

        public string FileName { get; }

        /// <summary>
        /// 从文件中创建<see cref="Hashes{T}"/>对象。
        /// </summary>
        /// <param name="filename">用于读取或存储所有hash的文件。</param>
        unsafe public UserDictionary(string filename) {
            if (File.Exists(filename)) {
                using var stream = File.OpenRead(filename);
                long hashCount = Math.DivRem(stream.Length, StructSize, out var rem);
                if (rem != 0 || hashCount > int.MaxValue) throw new ArgumentException("无效的users文件或已损坏", nameof(filename));
                prevHashCount = (int)hashCount;
                hashTable = new HashTable(Math.Max((int)hashCount, 1000));
                values = new List<PublicKeyStruct>(Math.Max((int)hashCount, 1000));
                Address address;
                PublicKeyStruct publicKey;
                while (hashCount-- > 0) {
                    stream.Read(new Span<byte>(&address, sizeof(Address)));
                    stream.Read(new Span<byte>(&publicKey, sizeof(PublicKeyStruct)));
                    hashTable.Add(&address);
                    values.Add(publicKey);
                }
            } else {
                hashTable = new HashTable(1000);
                values = new List<PublicKeyStruct>(1000);
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
                    fileStream.Write(new ReadOnlySpan<byte>(hashTable[i], sizeof(Address)));
                    PublicKeyStruct publicKey = values[i];
                    fileStream.Write(new ReadOnlySpan<byte>(&publicKey, sizeof(PublicKeyStruct)));
                }
                prevHashCount = currentCount;
            } finally {
                saving = 0;
            }
        }

        unsafe public Address GetAddress(int index) => *hashTable[index];

        public PublicKey GetPublicKey(int index) => values[index];

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
