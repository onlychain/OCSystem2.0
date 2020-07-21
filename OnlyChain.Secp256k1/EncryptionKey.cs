using OnlyChain.Secp256k1.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Secp256k1 {
    /// <summary>
    /// 用于p2p加密传输的密钥
    /// </summary>
    public sealed class EncryptionKey {
        internal U256 x, y;

        internal EncryptionKey(in U256 x, in U256 y) => (this.x, this.y) = (x, y);

        public void GetX(Span<byte> buffer) => x.CopyTo(buffer, true);
        public byte[] GetX() => x.ToArray(true);
        public void GetY(Span<byte> buffer) => y.CopyTo(buffer, true);
        public byte[] GetY() => y.ToArray(true);
    }
}
