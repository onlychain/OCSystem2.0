using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Coding {
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ErasureCodingIndex {
        public readonly int DataMissIndex;
        public readonly int ErasureCodeIndex;

        public ErasureCodingIndex(int dataMissIndex, int erasureCodeIndex) => (DataMissIndex, ErasureCodeIndex) = (dataMissIndex, erasureCodeIndex);

        public void Deconstruct(out int dataMissIndex, out int erasureCodeIndex) {
            dataMissIndex = DataMissIndex;
            erasureCodeIndex = ErasureCodeIndex;
        }

        public static implicit operator ErasureCodingIndex((int DataMissIndex, int ErasureCodeIndex) pair)
            => new ErasureCodingIndex(pair.DataMissIndex, pair.ErasureCodeIndex);
    }
}
