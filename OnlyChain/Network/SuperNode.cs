#nullable enable

using OnlyChain.Core;
using OnlyChain.Secp256k1;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    /// <summary>
    /// 存储竞选节点的连接信息。
    /// <para>只能用来发送消息或响应请求。</para>
    /// </summary>
    public sealed class SuperNode : IDisposable {
        public PublicKey PublicKey { get; }
        public IPEndPoint IPEndPoint { get; internal set; }
        public Socket? ClientSocket { get; internal set; }
        public bool IsReadOnly { get; }

        public bool Connected => ClientSocket?.Connected ?? false;


        public SuperNode(PublicKey publicKey, IPEndPoint endPoint, bool isReadOnly = false) {
            PublicKey = publicKey;
            IPEndPoint = endPoint;
            IsReadOnly = isReadOnly;
        }

        public Task ConnectAsync() {
            Reset();

            ClientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // 启用tcp心跳
            ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2); // 重试次数
            ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5); // 冷却间隔秒数
            ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1); // 重试间隔秒数
            return ClientSocket.ConnectAsync(IPEndPoint);
        }

        public void Reset() {
            if (IsReadOnly) throw new InvalidOperationException();

            if (ClientSocket is { }) {
                ClientSocket.Close();
                ClientSocket = null;
            }
        }

        public void Dispose() {
            if (IsReadOnly) return;

            ClientSocket?.Dispose();
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
            if (cancellationToken.IsCancellationRequested) return;
            if (Connected is false) return;

            byte[] lengthBuffer = ArrayPool<byte>.Shared.Rent(4);
            try {
                using var stream = new NetworkStream(ClientSocket, ownsSocket: false);
                BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, buffer.Length);
                ClientSocket!.NoDelay = false;
                await stream.WriteAsync(lengthBuffer, cancellationToken);
                ClientSocket.NoDelay = true;
                await stream.WriteAsync(buffer, cancellationToken);
            } catch { } finally {
                ArrayPool<byte>.Shared.Return(lengthBuffer);
            }
        }


    }
}
