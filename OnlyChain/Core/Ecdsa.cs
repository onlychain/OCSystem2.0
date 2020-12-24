using OnlyChain.Network.Objects;
using OnlyChain.Secp256k1;
using System;
using System.Collections.Generic;
using System.IO;

namespace OnlyChain.Core {
    unsafe public static class Ecdsa {
        public static Signature Sign(ReadOnlySpan<byte> privateKey, Bytes<Hash256> messageHash) {
            return Secp256k1.Secp256k1.Sign(privateKey, new ReadOnlySpan<byte>(&messageHash, sizeof(Hash256)));
        }

        public static bool Verify(PublicKey publicKey, Bytes<Hash256> messageHash, Signature signature) {
            return Secp256k1.Secp256k1.Verify(publicKey, new ReadOnlySpan<byte>(&messageHash, sizeof(Hash256)), signature);
        }

        public static Signature Sign(in this Serializer serializer, ReadOnlySpan<byte> privateKey, out Bytes<Hash256> hash) {
            hash = serializer.RawData.MessageHash();
            return Sign(privateKey, hash);
        }

        const string _pubkey_ = "pubkey";
        const string _sign_ = "sign";

        static byte[] SerializeForSign(BDict dict, PublicKey publicKey) {
            using var stream = new MemoryStream();
            var temp = new BDict();
            var sortedDict = new SortedDictionary<string, BObject>(dict, Comparer<string>.Create(StringComparer));
            sortedDict.Remove(_pubkey_);
            sortedDict.Remove(_sign_);

            var writeArgs = new BWriteArgs { Stream = stream, SortedKey = true };
            foreach (var (k, v) in sortedDict) {
                BString.WriteNoPrefix(stream, k);
                v.Write(ref writeArgs);
            }
            BString.WriteNoPrefix(stream, _pubkey_);
            new BBuffer(PublicKeyTool.ToArray(publicKey)).Write(stream);

            return stream.ToArray();


            static int StringComparer(string a, string b) {
                int cmp = a.Length.CompareTo(b.Length);
                if (cmp != 0) return cmp;
                return a.AsSpan().SequenceCompareTo(b);
            }
        }

        public static BDict Sign(BDict dict, ReadOnlySpan<byte> privateKey) {
            PublicKey publicKey = Secp256k1.Secp256k1.CreatePublicKey(privateKey);
            byte[] msgForSign = SerializeForSign(dict, publicKey);

            Bytes<Hash256> hash = HashTools.MessageHash(msgForSign);
            Signature sign = Sign(privateKey, hash);

            dict[_pubkey_] = publicKey.Serialize(compressed: true);
            dict[_sign_] = SignatureTool.ToArray(sign);
            return dict;
        }

        public static bool Verify(BDict dict) {
            try {
                var publicKey = GetPublicKey(dict);
                var sign = SignatureTool.Parse((byte[])dict[_sign_]);
                byte[] msgForSign = SerializeForSign(dict, publicKey);
                Bytes<Hash256> hash = HashTools.MessageHash(msgForSign);
                return Verify(publicKey, hash, sign);
            } catch {
                return false;
            }
        }

        public static Bytes<Address> GetAddress(BDict dict) {
            return GetPublicKey(dict).ToAddress();
        }

        public static PublicKey GetPublicKey(BDict dict) {
            return PublicKey.Parse((byte[])dict[_pubkey_]);
        }

        public static PublicKey RecoverPublicKey(Bytes<Hash256> message, Signature signature) {
            return Secp256k1.Secp256k1.RecoverPublicKey(new ReadOnlySpan<byte>(&message, sizeof(Hash256)), signature);
        }
    }
}
