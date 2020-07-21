#nullable enable

using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    /// <summary>
    /// 主要用于数据同步
    /// </summary>
    public sealed class TcpServer : IDisposable {
        private readonly IClient client;
        private readonly Socket tcpSocket;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public IPEndPoint IPEndPoint => (IPEndPoint)tcpSocket.LocalEndPoint;

        public TcpServer(IClient client, IPEndPoint bindEndPoint) {
            this.client = client;

            tcpSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // 启用tcp心跳
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2); // 重试次数
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10); // 冷却间隔秒数
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1); // 重试间隔秒数
            tcpSocket.Bind(bindEndPoint);
            tcpSocket.Listen(10);

            StartAccept();
        }

        private async void StartAccept() {
            while (!cancellationTokenSource.IsCancellationRequested) {
                try {
                    HandleClient(await tcpSocket.AcceptAsync());
                } catch { }
            }
        }

        enum Command : byte {
            QueryBlockFromHash = 0x01,
            QueryBlockFromHeight = 0x02,
            QueryAction = 0x03,
            QueryUser = 0x04,

        }

        private async void HandleClient(Socket client) {
            try {
                using var stream = new NetworkStream(client, ownsSocket: true) {
                    ReadTimeout = 2000
                };
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

                // 01 hash256 
                // 根据区块hash查询区块

                // 02 height(4 bytes)
                // 根据区块高度查询区块

                // 03 hash256
                // 根据action hash查询action

                // 04 address
                // 查询账户信息

                Command cmd = await stream.ReadStructAsync<Command>(cancellationTokenSource.Token);
                switch (cmd) {
                    case Command.QueryBlockFromHash:
                        Hash<Size256> blockHash = await stream.ReadStructAsync<Hash<Size256>>(cancellationTokenSource.Token);

                        break;
                    case Command.QueryBlockFromHeight:
                        uint blockHeight = (uint)IPAddress.NetworkToHostOrder(await stream.ReadStructAsync<int>(cancellationTokenSource.Token));

                        break;
                    case Command.QueryAction:
                        Hash<Size256> actionHash = await stream.ReadStructAsync<Hash<Size256>>(cancellationTokenSource.Token);

                        break;
                    case Command.QueryUser:
                        Address userAddress = await stream.ReadStructAsync<Address>(cancellationTokenSource.Token);

                        break;
                }
            } catch {

            }
        }


        #region IDisposable Support
        private bool isDisposed = false;

        void Dispose(bool disposing) {
            if (!isDisposed) {
                cancellationTokenSource.Cancel();
                if (disposing) {
                    GC.SuppressFinalize(this);
                }
                isDisposed = true;
            }
        }

        ~TcpServer() {
            Dispose(false);
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
