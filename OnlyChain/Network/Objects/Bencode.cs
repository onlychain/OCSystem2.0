using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    unsafe public static class Bencode {
        public static BObject Decode(ReadOnlySpan<byte> data, string prefix = null) {
            try {
                prefix ??= string.Empty;
                var prefixBytes = Encoding.UTF8.GetBytes(prefix);
                if (!data[..prefix.Length].SequenceEqual(prefixBytes) || data[prefix.Length] != 0) throw null;
                return new BDecoder(data, prefix.Length + 1).Decode();
            } catch {
                throw new FormatException();
            }
        }

        public static void Encode(Stream stream, BObject obj, string prefix = null) {
            if (prefix is { }) {
                stream.Write(Encoding.UTF8.GetBytes(prefix));
            }
            stream.WriteByte(0);
            obj.Write(stream);
        }

        public static byte[] Encode(BObject obj, string prefix = null) {
            using var mem = new MemoryStream(1500);
            Encode(mem, obj, prefix);
            return mem.ToArray();
        }

        private ref struct BDecoder {
            private readonly ReadOnlySpan<byte> data;
            private int i;

            public BDecoder(ReadOnlySpan<byte> data, int i) {
                this.data = data;
                this.i = i;
            }

            private ReadOnlySpan<byte> ParseSpan() {
                var strLength = data[i..].ReadVarUInt(out var len);
                i += len;
                if (strLength > (ulong)(data.Length - i)) throw new FormatException();
                var span = data.Slice(i, (int)strLength);
                i += (int)strLength;
                return span;
            }

            private string ParseString() {
                int len = 0;
                while (data[i + len] != 0) len++;
                var s = Encoding.UTF8.GetString(data.Slice(i, len));
                i += len + 1;
                return s;
            }

            private int ParseCount() {
                var count = data[i..].ReadVarUInt(out var len);
                if (count >= int.MaxValue) throw new FormatException();
                i += len;
                return (int)count;
            }

            unsafe public BObject Decode() {
                switch (data[i++]) {
                    case BDict.PrefixChar: {
                        var dict = new Dictionary<string, BObject>();
                        int count = ParseCount();
                        for (int n = 0; n < count; n++) {
                            var key = ParseString();
                            var value = Decode();
                            dict.Add(key, value);
                        }
                        return new BDict(dict);
                    }
                    case BList.PrefixChar: {
                        var list = new List<BObject>();
                        int count = ParseCount();
                        for (int n = 0; n < count; n++) {
                            var value = Decode();
                            list.Add(value);
                        }
                        return new BList(list);
                    }
                    case BInt.PrefixChar: {
                        var @int = new BInt(data[i..].ReadVarInt(out var len));
                        i += len;
                        return @int;
                    }
                    case BUInt.PrefixChar: {
                        var @uint = new BUInt(data[i..].ReadVarUInt(out var len));
                        i += len;
                        return @uint;
                    }
                    case BBool.FalseChar:
                        return BBool.False;
                    case BBool.TrueChar:
                        return BBool.True;
                    case BString.PrefixChar:
                        return new BString(ParseString());
                    case BBuffer.PrefixChar:
                        return new BBuffer(ParseSpan());
                    case BAddress.PrefixChar: {
                        if (i + sizeof(Address) > data.Length) throw new FormatException();
                        var addr = new BAddress(new Address(data.Slice(i, sizeof(Address))));
                        i += sizeof(Address);
                        return addr;
                    }
                    default: throw new FormatException();
                }
            }
        }
    }
}
