using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    public static class Bencode {
        public static BObject Decode(Stream stream, string prefix = null, int maxReadSize = 4096) {
            try {
                prefix ??= string.Empty;
                byte[] prefixBytes = Encoding.UTF8.GetBytes(prefix);

                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                byte[] otherBytes = reader.ReadBytes(prefixBytes.Length + 1);

                if (!otherBytes.AsSpan(0, prefixBytes.Length).SequenceEqual(prefixBytes) || otherBytes[prefixBytes.Length] != 0) throw new FormatException();

                return DecodeNoPrefix(stream, maxReadSize);
            } catch (Exception e) when (e is not FormatException) {
                throw new FormatException();
            }
        }

        /// <summary>
        /// 解析BObject（无网络前缀）
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="maxReadSize">对于所有<see cref="BBuffer"/>允许的最大字节数</param>
        /// <returns></returns>
        public static BObject DecodeNoPrefix(Stream stream, int maxReadSize = 4096) {
            return new BDecoder(stream, maxReadSize).Decode();
        }

        public static async ValueTask<BObject> DecodeAsync(Stream stream, string prefix = null, int maxReadSize = 4096, CancellationToken cancellationToken = default) {
            try {
                prefix ??= string.Empty;
                byte[] prefixBytes = Encoding.UTF8.GetBytes(prefix);
                byte[] otherBytes = await stream.ReadBytesAsync(prefixBytes.Length + 1, cancellationToken);

                if (!otherBytes.AsSpan(0, prefixBytes.Length).SequenceEqual(prefixBytes) || otherBytes[prefixBytes.Length] != 0) throw new FormatException();

                return await DecodeNoPrefixAsync(stream, maxReadSize, cancellationToken);
            } catch (Exception e) when (e is not FormatException) {
                throw new FormatException();
            }
        }

        public static ValueTask<BObject> DecodeNoPrefixAsync(Stream stream, int maxReadSize = 4096, CancellationToken cancellationToken = default) {
            return new BDecoderAsync(stream, maxReadSize, cancellationToken).Decode();
        }

        public static void Encode(Stream stream, BObject obj, string prefix = null) {
            if (prefix is not null) {
                stream.Write(Encoding.UTF8.GetBytes(prefix));
            }
            stream.WriteByte(0);

            EncodeNoPrefix(stream, obj);
        }

        public static byte[] Encode(BObject obj, string prefix = null) {
            using var mem = new MemoryStream(1500);
            Encode(mem, obj, prefix);
            return mem.ToArray();
        }

        public static void EncodeNoPrefix(Stream stream, BObject obj) {
            obj.Write(stream);
        }

        public static byte[] EncodeNoPrefix(BObject obj) {
            using var mem = new MemoryStream(1500);
            EncodeNoPrefix(mem, obj);
            return mem.ToArray();
        }

        public static ValueTask EncodeAsync(Stream stream, BObject obj, string prefix = null, CancellationToken cancellationToken = default) {
            return stream.WriteAsync(Encode(obj, prefix), cancellationToken);
        }

        public static ValueTask EncodeNoPrefixAsync(Stream stream, BObject obj, CancellationToken cancellationToken = default) {
            return stream.WriteAsync(EncodeNoPrefix(obj), cancellationToken);
        }

        /// <summary>
        /// 用于减少递归产生的栈开销
        /// </summary>
        private ref struct BDecoder {
            private readonly BinaryReader reader;
            private readonly int maxSize;
            private int index;

            public BDecoder(Stream stream, int maxSize = int.MaxValue) {
                reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                this.maxSize = maxSize;
                index = 0;
            }

            private byte ReadByte() {
                if (index >= maxSize) throw new FormatException();
                byte b = reader.ReadByte();
                index++;
                return b;
            }

            private byte[] ParseSpan() {
                var length = reader.BaseStream.ReadVarUInt(out int readBytes);
                if (length > int.MaxValue) throw new FormatException();
                if (index + readBytes + (int)length > maxSize) throw new FormatException();
                byte[] span = reader.ReadBytes((int)length);
                index += readBytes + (int)length;
                return span;
            }

            private string ParseString() {
                return Encoding.UTF8.GetString(ParseSpan());
            }

            private int ParseCount() {
                var count = reader.BaseStream.ReadVarUInt(out int readBytes);
                if (index + readBytes > maxSize) throw new FormatException();
                index += readBytes;
                return (int)count;
            }

            unsafe public BObject Decode() {
                switch (ReadByte()) {
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
                        var result = new BInt(reader.BaseStream.ReadVarInt(out int readBytes));
                        if (index + readBytes > maxSize) throw new FormatException();
                        index += readBytes;
                        return result;
                    }
                    case BUInt.PrefixChar: {
                        var result = new BUInt(reader.BaseStream.ReadVarUInt(out int readBytes));
                        if (index + readBytes > maxSize) throw new FormatException();
                        index += readBytes;
                        return result;
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
                        Bytes<Address> addr = new Bytes<Address>(reader.ReadBytes(sizeof(Address)));
                        index += sizeof(Address);
                        return new BAddress(addr);
                    }
                    default: throw new FormatException();
                }
            }
        }

        private sealed class BDecoderAsync {
            private readonly Stream stream;
            private readonly CancellationToken cancellationToken;
            private readonly int maxSize;
            private int index;

            public BDecoderAsync(Stream stream, int maxSize = int.MaxValue, CancellationToken cancellationToken = default) {
                this.stream = stream;
                this.cancellationToken = cancellationToken;
                this.maxSize = maxSize;
                index = 0;
            }

            private void CheckSize(int size) {
                if (index + size > maxSize) throw new FormatException();
            }

            private async ValueTask<byte> ReadByte() {
                CheckSize(1);
                byte b = await stream.ReadByteAsync(cancellationToken);
                index++;
                return b;
            }

            private async ValueTask<byte[]> ParseSpan() {
                var length = await ParseCount();
                CheckSize(length);
                byte[] span = await stream.ReadBytesAsync(length, cancellationToken);
                index += length;
                return span;
            }

            private async ValueTask<string> ParseString() {
                return Encoding.UTF8.GetString(await ParseSpan());
            }

            private async ValueTask<int> ParseCount() {
                var (count, readBytes) = await stream.ReadVarUIntAsync(cancellationToken);
                if (count >= (ulong)maxSize) throw new FormatException();
                CheckSize(readBytes);
                index += readBytes;
                return (int)count;
            }

            public async ValueTask<BObject> Decode() {
                switch (await ReadByte()) {
                    case BDict.PrefixChar: {
                        var dict = new Dictionary<string, BObject>();
                        int count = await ParseCount();
                        for (int n = 0; n < count; n++) {
                            var key = await ParseString();
                            var value = await Decode();
                            dict.Add(key, value);
                        }
                        return new BDict(dict);
                    }
                    case BList.PrefixChar: {
                        var list = new List<BObject>();
                        int count = await ParseCount();
                        for (int n = 0; n < count; n++) {
                            var value = await Decode();
                            list.Add(value);
                        }
                        return new BList(list);
                    }
                    case BInt.PrefixChar: {
                        var (result, readBytes) = await stream.ReadVarIntAsync(cancellationToken);
                        CheckSize(readBytes);
                        index += readBytes;
                        return result;
                    }
                    case BUInt.PrefixChar: {
                        var (result, readBytes) = await stream.ReadVarUIntAsync(cancellationToken);
                        CheckSize(readBytes);
                        index += readBytes;
                        return result;
                    }
                    case BBool.FalseChar:
                        return BBool.False;
                    case BBool.TrueChar:
                        return BBool.True;
                    case BString.PrefixChar:
                        return new BString(await ParseString());
                    case BBuffer.PrefixChar:
                        return new BBuffer(await ParseSpan());
                    case BAddress.PrefixChar: {
                        CheckSize(AddressSize());
                        Bytes<Address> addr = new Bytes<Address>(await stream.ReadBytesAsync(AddressSize(), cancellationToken));
                        index += AddressSize();
                        return new BAddress(addr);
                    }
                    default: throw new FormatException();
                }

                unsafe static int AddressSize() => sizeof(Address);
            }
        }
    }
}
