using System;
using System.Security.Cryptography;
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
            } while (k.IsZero || k >= U256N.N);
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
            if (k.IsZero || k >= U256N.N) throw new InvalidPrivateKeyException();
            var retPoint = (Point)EllipticCurve.MulG(k);
            return new PublicKey(retPoint.X, retPoint.Y);
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

            var dA = new U256N(privateKey);
            var msg = new U256N(message);
            Span<byte> tempPrivateKey = stackalloc byte[32];
            CreatePrivateKey(tempPrivateKey);
            var k = new U256N(tempPrivateKey);
            var p = (Point)EllipticCurve.MulG(k);
            U256N R = p.X.Value;
            U256N S = (msg + dA * R) / k;
            if (p.Y.Value.v0 % 2 != 0) {
                S = -S;
            }
            return new Signature(R, S);
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

            var msg = new U256N(message);
            var S_inv = ~(U256N)signature.S;
            var u1 = S_inv * msg;
            var u2 = S_inv * signature.R;
            var P = EllipticCurve.MulG(u1) + new JacobianPoint(publicKey.X, publicKey.Y) * u2;
            return P.X == signature.R * (P.Z ^ 2);
        }

        /// <summary>
        /// 使用自己的私钥与对方公钥进行密钥交换（私钥A×公钥B = 私钥B×公钥A）
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        unsafe public static EncryptionKey CreateEncryptionKey(ReadOnlySpan<byte> privateKey, PublicKey publicKey) {
            if (privateKey.Length != 32) throw new InvalidPrivateKeyException("私钥长度必须是32字节");

            var k = new U256(privateKey);
            if (k.IsZero || k >= U256N.N) throw new InvalidPrivateKeyException();
            var p = (Point)(new JacobianPoint(publicKey.X, publicKey.Y) * k);
            return new EncryptionKey(p.X, p.Y);
        }

        /// <summary>
        /// 从(消息,签名)恢复公钥
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        unsafe public static PublicKey RecoverPublicKey(ReadOnlySpan<byte> message, Signature signature) {
            if (message.Length != 32) throw new InvalidMessageException("消息长度必须是32字节");

            U256N s = signature.S;
            var m = new U256N(message);
            var rY = EllipticCurve.GetY(signature.R);
            if (rY.Value.v0 % 2 != 0) {
                s = -s;
            }

            var rP = new JacobianPoint(signature.R, rY);
            var r_inv = ~new U256N(signature.R);
            var u1 = EllipticCurve.MulG(-m * r_inv);
            var u2 = s * r_inv;
            var p = (Point)(rP * u2 + u1);
            return new PublicKey(p.X, p.Y);
        }
    }
}
