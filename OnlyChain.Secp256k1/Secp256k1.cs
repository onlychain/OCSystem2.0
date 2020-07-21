using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using OnlyChain.Secp256k1.Math;

namespace OnlyChain.Secp256k1 {
    public static class Secp256k1 {
        static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        unsafe static void Clear(U256* v) {
            *v = default;
        }

        /// <summary>
        /// 创建私钥
        /// </summary>
        /// <param name="outPrivateKey">用于存放私钥的缓冲区</param>
        unsafe public static void CreatePrivateKey(Span<byte> outPrivateKey) {
            if (outPrivateKey.Length < 32) throw new ArgumentException("缓冲区至少要32字节", nameof(outPrivateKey));
            var privateKey = outPrivateKey.Slice(0, 32);
            U256 k;
            do {
                rng.GetBytes(privateKey);
                k = new U256(privateKey, bigEndian: true);
            } while (k.IsZero || k >= ModN.N);

            Clear(&k);
        }

        /// <summary>
        /// 创建私钥
        /// </summary>
        /// <returns></returns>
        public static byte[] CreatePrivateKey() {
            var privateKey = new byte[32];
            CreatePrivateKey(privateKey);
            return privateKey;
        }

        /// <summary>
        /// 根据私钥创建公钥
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        unsafe public static PublicKey CreatePublicKey(ReadOnlySpan<byte> privateKey) {
            if (privateKey.Length != 32) throw new InvalidPrivateKeyException("私钥长度必须是32字节");
            var k = new U256(privateKey, bigEndian: true);
            if (k.IsZero || k >= ModN.N) throw new InvalidPrivateKeyException();
            var retPoint = ModP.MulG(k);
            Clear(&k);
            return new PublicKey(ModP.ToU256(retPoint.X), ModP.ToU256(retPoint.Y));
        }

        /// <summary>
        /// 使用私钥对消息进行签名
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        unsafe public static Signature Sign(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> message) {
            if (privateKey.Length != 32) throw new InvalidPrivateKeyException("私钥长度必须是32字节");
            if (message.Length != 32) throw new InvalidMessageException("消息长度必须是32字节");
            var dA = ModN.U256(privateKey, bigEndian: true);
            var msg = ModN.U256(message, bigEndian: true);
            var tempPrivKey = CreatePrivateKey();
            var tempPubKey = CreatePublicKey(tempPrivKey);
            var k = new U256(tempPrivKey, bigEndian: true);
            var S = ModN.Div(ModN.Add(msg, ModN.Mul(dA, tempPubKey.x)), k);
            tempPrivKey.AsSpan().Clear();
            Clear(&dA);
            Clear(&k);
            return new Signature(tempPubKey.x, S);
        }

        public static Task<Signature> SignAsync(ReadOnlyMemory<byte> privateKey, ReadOnlyMemory<byte> message) {
            return Task.Run(() => Sign(privateKey.Span, message.Span));
        }

        /// <summary>
        /// 使用公钥验证消息的签名是否正确
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="message"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool Verify(PublicKey publicKey, ReadOnlySpan<byte> message, Signature signature) {
            if (message.Length != 32) throw new InvalidMessageException("消息长度必须是32字节");
            var msg = ModN.U256(message, bigEndian: true);
            var S_inv = ModN.Inverse(signature.S);
            var u1 = ModN.Mul(S_inv, msg);
            var u2 = ModN.Mul(S_inv, signature.R);
            var P = ModP.Add(ModP.MulG(u1), ModP.Mul(publicKey.ToPoint(), u2));
            return ModP.Equal(P.X, signature.R);
        }

        public static Task<bool> VerifyAsync(PublicKey publicKey, ReadOnlyMemory<byte> message, Signature signature) {
            return Task.Run(() => Verify(publicKey, message.Span, signature));
        }

        /// <summary>
        /// 使用自己的私钥与对方公钥进行密钥交换（私钥A×公钥B = 私钥B×公钥A）
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        unsafe public static EncryptionKey CreateEncryptionKey(ReadOnlySpan<byte> privateKey, PublicKey publicKey) {
            if (privateKey.Length != 32) throw new InvalidPrivateKeyException("私钥长度必须是32字节");
            var k = new U256(privateKey, bigEndian: true);
            if (k.IsZero || k >= ModN.N) throw new InvalidPrivateKeyException();

            var p = ModP.Mul(publicKey.ToPoint(), k);
            Clear(&k);
            return new EncryptionKey(ModP.ToU256(p.X), ModP.ToU256(p.Y));
        }
    }
}
