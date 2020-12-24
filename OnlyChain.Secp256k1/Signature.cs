using OnlyChain.Secp256k1.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Secp256k1 {
    public sealed class Signature {
        public readonly U256 R, S;

        public void GetR(Span<byte> buffer) => R.CopyTo(buffer, bigEndian: true);
        public byte[] GetR() => R.ToArray(bigEndian: true);
        public void GetS(Span<byte> buffer) => S.CopyTo(buffer, bigEndian: true);
        public byte[] GetS() => S.ToArray(bigEndian: true);

        public Signature(in U256 r, in U256 s) => (R, S) = (r, s);
    }
}
