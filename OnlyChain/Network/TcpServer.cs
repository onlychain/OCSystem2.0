#nullable enable

using OnlyChain.Core;
using OnlyChain.Network.Objects;
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

        public IPEndPoint IPEndPoint => (IPEndPoint)tcpSocket.LocalEndPoint!;

        public TcpServer(IClient client) {
            this.client = client;

            tcpSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //tcpSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // 启用tcp心跳
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2); // 重试次数
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 60); // 冷却间隔秒数
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 3); // 重试间隔秒数
            tcpSocket.Bind(client.EndPoint);
            tcpSocket.Listen(20);

            StartAccept();
        }

        private async void StartAccept() {
            try {
                while (!client.CloseCancellationToken.IsCancellationRequested) {
                    var socketClient = await tcpSocket.AcceptAsync();
                    HandleClient(socketClient);
                }
            } catch { }
        }

        private async void HandleClient(Socket client) {
            try {
                using var stream = new NetworkStream(client, ownsSocket: true) {
                    ReadTimeout = 5000,
                    WriteTimeout = 5000,
                };

                BDict dict = (BDict)await Bencode.DecodeAsync(stream, this.client.NetworkPrefix);
                await this.client.P2P.TcpServer_GetValue(stream, dict);
            } catch {

            }
        }


        #region IDisposable Support
        private bool isDisposed = false;

        void Dispose(bool disposing) {
            if (!isDisposed) {
                try {
                    tcpSocket.Shutdown(SocketShutdown.Both);
                } catch { }
                try {
                    tcpSocket.Close();
                } catch { }

                if (disposing) {
                    tcpSocket.Dispose();

                }
                isDisposed = true;
            }
        }

        ~TcpServer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
