#nullable enable

using OnlyChain.Coding;
using OnlyChain.Secp256k1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class BlockChipCollection : IReadOnlyList<byte[]?> {
        private readonly TaskCompletionSource<object?> restoreTaskSource = new TaskCompletionSource<object?>();
        private readonly byte[]?[] datas;
        private int count;
        private int writeCount = 0;
        private bool isReadOnly = false;

        public readonly Address Address;
        public readonly PublicKey PublicKey;
        public readonly Hash<Size160> Id;
        public readonly int TotalCount;
        public readonly int RestoreCount;
        public readonly int ChipDataBytes;
        public readonly int BlockBytes;

        public bool CanRestore { get; private set; } = false;

        public BlockChipCollection(BlockChip firstChip, Address address, PublicKey publicKey) {
            Id = firstChip.Id;
            TotalCount = firstChip.TotalCount;
            RestoreCount = firstChip.RestoreCount;
            datas = new byte[]?[TotalCount];
            datas[firstChip.Index] = firstChip.Data;
            ChipDataBytes = firstChip.Data.Length;
            BlockBytes = firstChip.BlockBytes;

            if (firstChip.Verify(publicKey) is false)
                throw new InvalidBlockChipException();

            Address = address;
            PublicKey = publicKey;

            count = 1;
        }

        public void Add(BlockChip blockChip) {
            if (isReadOnly && writeCount == 0) return;

            Interlocked.Increment(ref writeCount);
            try {
                if (blockChip.RestoreCount != RestoreCount) goto Invalid;
                if (blockChip.TotalCount != TotalCount) goto Invalid;
                if (blockChip.Data.Length != ChipDataBytes) goto Invalid;
                if (blockChip.BlockBytes != BlockBytes) goto Invalid;
                if (datas[blockChip.Index] is { }) goto Invalid;
                if (blockChip.Verify(PublicKey) is false) goto Invalid;

                datas[blockChip.Index] = blockChip.Data;
                if (Interlocked.Increment(ref count) == RestoreCount) {
                    CanRestore = true;
                }
                return;
            } finally {
                if (Interlocked.Decrement(ref writeCount) == 0 && CanRestore) {
                    isReadOnly = true;
                    restoreTaskSource.TrySetResult(null);
                }
            }

        Invalid:
            throw new InvalidBlockChipException();
        }

        public async Task<Block> RestoreAsync() {
            await restoreTaskSource.Task;
            return Restore();
        }

        unsafe private Block Restore() {
            byte[]?[] datas = (byte[]?[])this.datas.Clone();
            var mapIndexList = new List<ErasureCodingIndex>();
            var mapIndexes = stackalloc int[RestoreCount];
            int ecPointer = 0;
            for (int i = 0; i < RestoreCount; i++) {
                if (datas[i] is null) {
                    while (datas[RestoreCount + ecPointer] is null) ecPointer++;
                    mapIndexes[i] = RestoreCount + ecPointer;
                    mapIndexList.Add(new ErasureCodingIndex(i, ecPointer++));
                } else {
                    mapIndexes[i] = i;
                }
            }

            byte[] buffer = new byte[RestoreCount * ChipDataBytes];
            int dstPointer = 0;
            for (int i = 0; i < RestoreCount; i++) {
                for (int j = 0; j < ChipDataBytes; j++) {
                    buffer[dstPointer++] = datas[mapIndexes[i]]![j];
                }
            }
            ErasureCoding.Decode(buffer, RestoreCount, mapIndexList.ToArray());
            return new Block(buffer.AsSpan(0, BlockBytes));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<挂起>")]
        public byte[]? this[int index] => datas[index];

        public int Count => TotalCount;

        public IEnumerator<byte[]?> GetEnumerator() => datas.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
