using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    [StructLayout(LayoutKind.Sequential)]
    public struct PublicKeyStruct {
        public Secp256k1.Math.U256 X, Y;

        public PublicKeyStruct(Secp256k1.Math.U256 x, Secp256k1.Math.U256 y) => (X, Y) = (x, y);
        public PublicKeyStruct(PublicKey publicKey) : this(publicKey.X, publicKey.Y) { }

        public static implicit operator PublicKeyStruct(PublicKey publicKey) => new PublicKeyStruct(publicKey);
        public static implicit operator PublicKey(PublicKeyStruct publicKey) => new PublicKey(publicKey.X, publicKey.Y);
    }
}
