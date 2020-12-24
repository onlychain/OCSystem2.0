#nullable enable

using OnlyChain.Core;
using OnlyChain.Secp256k1;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    /// <summary>
    /// 表示用于通信的超级节点。
    /// </summary>
    /// <remarks>
    /// 请使用<see cref="Communication.P2P"/>的相关方法。
    /// </remarks>
    public sealed class SuperPeer : IDisposable {
        public SuperNode SuperNode { get; }
        public Bytes<Address> Address => SuperNode.Address;
        public PublicKey PublicKey => SuperNode.PublicKey;
        public IPEndPoint IPEndPoint { get => SuperNode.IPEndPoint; internal set => SuperNode.IPEndPoint = value; }
        public Socket? Socket { get; private set; }
        public SemaphoreSlim? SendLock { get; private set; }

        public bool IsOnline => Socket?.IsOnline() ?? false;

        public SuperPeer(SuperNode superNode) {
            SuperNode = superNode;
        }

        public SuperPeer(PublicKey publicKey, IPEndPoint ep) : this(new SuperNode(publicKey, ep)) { }

        public Task InitSocket() {
            Reset();

            SendLock = new SemaphoreSlim(1, 1);
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // 启用tcp心跳
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2); // 重试次数
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5); // 冷却间隔秒数
            Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1); // 重试间隔秒数
            return Socket.ConnectAsync(IPEndPoint);
        }

        public void Reset() {
            Socket?.Dispose();
            SendLock?.Dispose();
            Socket = null;
            SendLock = null;
        }

        public void Dispose() {
            Reset();
        }
    }
}
