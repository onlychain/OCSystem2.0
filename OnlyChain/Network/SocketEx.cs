using System;
using System.Net.Sockets;

namespace OnlyChain.Network {
    public static class SocketEx {
        public static bool IsOnline(this Socket socket) => socket.Connected && (socket.Available > 0 || !socket.Poll(50, SelectMode.SelectRead) || socket.Available > 0);
    }
}
