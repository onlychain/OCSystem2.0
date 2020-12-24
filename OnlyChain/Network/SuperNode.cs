#nullable enable

using OnlyChain.Core;
using OnlyChain.Secp256k1;
using System.Net;

namespace OnlyChain.Network {
    /// <summary>
    /// 存储竞选节点的连接信息。
    /// </summary>
    public sealed class SuperNode {
        public Bytes<Address> Address { get; }
        public PublicKey PublicKey { get; }
        public IPEndPoint IPEndPoint { get; internal set; }

        
        public SuperNode(PublicKey publicKey, IPEndPoint endPoint) {
            Address = publicKey.ToAddress();
            PublicKey = publicKey;
            IPEndPoint = endPoint;
        }

        //public Task ConnectAsync() {
        //    Reset();

        //    ClientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        //    ClientSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        //    ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // 启用tcp心跳
        //    ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2); // 重试次数
        //    ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5); // 冷却间隔秒数
        //    ClientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1); // 重试间隔秒数
        //    return ClientSocket.ConnectAsync(IPEndPoint);
        //}

        //public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
        //    if (cancellationToken.IsCancellationRequested) return;
        //    if (Connected is false) return;

        //    Debug.Assert(ClientSocket is not null);

        //    byte[] lengthBuffer = ArrayPool<byte>.Shared.Rent(4);
        //    try {
        //        using var stream = new NetworkStream(ClientSocket, ownsSocket: false);
        //        BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, buffer.Length);
        //        ClientSocket.NoDelay = false;
        //        await stream.WriteAsync(lengthBuffer, cancellationToken);
        //        ClientSocket.NoDelay = true;
        //        await stream.WriteAsync(buffer, cancellationToken);
        //    } catch { } finally {
        //        ArrayPool<byte>.Shared.Return(lengthBuffer);
        //    }
        //}


    }
}
