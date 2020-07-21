#nullable enable

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
        public static async ValueTask FillAsync(this NetworkStream networkStream, Memory<byte> buffer, CancellationToken cancellationToken = default) {
            Stopwatch? timer = null;
            int timeout = networkStream.ReadTimeout;
            if (timeout != -1) {
                timer = new Stopwatch();
                timer.Start();
            }

            int index = 0;
            while (index < buffer.Length) {
                if (timer is { } && (int)timer.ElapsedMilliseconds > timeout)
                    throw new SocketException((int)SocketError.TimedOut);

                int readBytes = await networkStream.ReadAsync(buffer[index..], cancellationToken);
                if (readBytes == 0) throw new SocketException((int)SocketError.ConnectionAborted);
                index += readBytes;
            }
        }

        public static async ValueTask<T> ReadStructAsync<T>(this NetworkStream networkStream, CancellationToken cancellationToken = default) where T : unmanaged {
            unsafe static int StructSize() => sizeof(T);

            var buffer = ArrayPool<byte>.Shared.Rent(StructSize());
            try {
                await FillAsync(networkStream, buffer.AsMemory(0, StructSize()), cancellationToken);
                return MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, T>(buffer.AsSpan(0, StructSize())));
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
