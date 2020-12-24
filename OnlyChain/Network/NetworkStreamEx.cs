#nullable enable

using OnlyChain.Core;
using OnlyChain.Network.Objects;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public static class NetworkStreamEx {
        public static async ValueTask<T> ReadStructAsync<T>(this Stream networkStream, CancellationToken cancellationToken = default) where T : unmanaged {
            unsafe static int StructSize() => sizeof(T);

            var buffer = ArrayPool<byte>.Shared.Rent(StructSize());
            try {
                await ReadBytesAsync(networkStream, buffer.AsMemory(0, StructSize()), cancellationToken);
                return MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, T>(buffer.AsSpan(0, StructSize())));
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static async ValueTask ReadBytesAsync(this Stream networkStream, Memory<byte> buffer, CancellationToken cancellationToken = default) {
            int index = 0;
            while (index < buffer.Length) {
                int readBytes = await networkStream.ReadAsync(buffer[index..], cancellationToken);
                if (readBytes == 0) throw new EndOfStreamException();
                index += readBytes;
            }
        }

        public static async ValueTask<byte[]> ReadBytesAsync(this Stream networkStream, int count, CancellationToken cancellationToken = default) {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return Array.Empty<byte>();
            var result = new byte[count];
            await ReadBytesAsync(networkStream, result, cancellationToken);
            return result;
        }

        public static async ValueTask<byte> ReadByteAsync(this Stream networkStream, CancellationToken cancellationToken = default) {
            var buffer = ArrayPool<byte>.Shared.Rent(1);
            try {
                int readBytes = await networkStream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
                if (readBytes == 0) throw new EndOfStreamException();
                return buffer[0];
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static ValueTask WriteByteAsync(this Stream networkStream, byte value, CancellationToken cancellationToken = default) {
            return networkStream.WriteAsync(new byte[] { value }, cancellationToken);
        }

        public static async ValueTask<(ulong Value, int ByteCount)> ReadVarUIntAsync(this Stream stream, CancellationToken cancellationToken = default) {
            ulong r = 0;
            int shift = 0;
            int v;
            for (int j = 0; j < 8; j++, shift += 7) {
                v = await ReadByteAsync(stream, cancellationToken);
                r |= ((ulong)v & 0x7f) << shift;
                if ((v & 0x80) == 0) {
                    return (r, j + 1);
                }
            }

            v = await ReadByteAsync(stream, cancellationToken);
            r |= (ulong)v << 56;
            return (r, 9);
        }

        public static async ValueTask<(long Value, int ByteCount)> ReadVarIntAsync(this Stream stream, CancellationToken cancellationToken = default) {
            (ulong r, int byteCount) = await ReadVarUIntAsync(stream, cancellationToken);
            if (byteCount == 9) return ((long)r, 9);
            int shift = 64 - byteCount * 7;
            return ((long)r << shift >> shift, byteCount);
        }

        public static async ValueTask<int> WriteVarUIntAsync(this Stream stream, ulong value, CancellationToken cancellationToken = default) {
            var buffer = ArrayPool<byte>.Shared.Rent(9);
            try {
                int len = buffer.AsSpan().WriteVarUInt(value);
                await stream.WriteAsync(buffer.AsMemory(0, len), cancellationToken);
                return len;
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static async ValueTask<int> WriteVarIntAsync(this Stream stream, long value, CancellationToken cancellationToken = default) {
            var buffer = ArrayPool<byte>.Shared.Rent(9);
            try {
                int len = buffer.AsSpan().WriteVarInt(value);
                await stream.WriteAsync(buffer.AsMemory(0, len), cancellationToken);
                return len;
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
