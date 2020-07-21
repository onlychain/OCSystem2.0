#nullable enable

using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using OnlyChain.Core;
using System.IO;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Channels;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;

namespace OnlyChain.Network {
    [System.Diagnostics.DebuggerDisplay("{Address}, port: {Port}")]
    public sealed class Client : IClient, IAsyncDisposable {
        static readonly Random random = new Random();

        readonly TimeSpan refreshKBucketTimeSpan = TimeSpan.FromSeconds(20);
        static readonly TimeSpan broadcastTimeout = TimeSpan.FromHours(2);

        private readonly Timer refreshKBucketTimer;
        private readonly CancellationTokenSource closeCancelTokenSource = new CancellationTokenSource();
        private readonly Dictionary<Address, DateTime> broadcastIdRecord = new Dictionary<Address, DateTime>();
        private readonly UdpServer udpServer;
        private readonly TcpServer tcpServer;

        public string? NetworkPrefix { get; }
        public Address Address { get; }
        public IPEndPoint UdpEndPoint => udpServer.IPEndPoint;
        public IPEndPoint TcpEndPoint => tcpServer.IPEndPoint;
        public KBucket Nodes { get; }
        public CancellationToken CloseCancellationToken => closeCancelTokenSource.Token;
        public BlockChainSystem System { get; }

        public event EventHandler<BroadcastEventArgs> ReceiveBroadcast = null!;
        public event EventHandler<GetValueEventArgs> ResponseGetValue = null!;

        public Client(Address address, IPAddress bindIPAddress, int udpPort, int tcpPort, string? networkPrefix = null, IEnumerable<IPEndPoint>? seeds = null) {
            Address = address;
            NetworkPrefix = networkPrefix;
            System = new BlockChainSystem(this, networkPrefix ?? "main");

            udpServer = new UdpServer(this, new IPEndPoint(bindIPAddress, udpPort));
            tcpServer = new TcpServer(this, new IPEndPoint(bindIPAddress, tcpPort));
            Nodes = new KBucket(16, address, Ping);

            refreshKBucketTimeSpan = TimeSpan.FromSeconds(random.NextDouble() * 1 + 7);
            refreshKBucketTimer = new Timer(async delegate {
                if (closeCancelTokenSource.IsCancellationRequested) return;
                var randomAddress = Address.Random();
                var neighborNodes = Nodes.FindNode(randomAddress, 10);
                var tasks = Array.ConvertAll(neighborNodes, node => FindNode(randomAddress, Nodes.K, node.IPEndPoint, hasRemoteNode: false, refreshKBucket: true));

                var now = DateTime.Now;
                lock (broadcastIdRecord) {
                    var removeKeys = new List<Address>();
                    foreach (var (id, time) in broadcastIdRecord) {
                        if (now - time >= broadcastTimeout) removeKeys.Add(id);
                        else break;
                    }
                    foreach (var id in removeKeys) {
                        broadcastIdRecord.Remove(id);
                    }
                }

                await tasks.WhenAll();
            }, null, refreshKBucketTimeSpan, refreshKBucketTimeSpan);


            Task? lookupTask = null;
            foreach (var seed in seeds ?? Enumerable.Empty<IPEndPoint>()) {
                var addresses = new SortedSet<Address>(Comparer<Address>.Create((a, b) => (a ^ Address).CompareTo(b ^ Address)));
                const int findCount = 10;

                async Task Find(IPEndPoint remote, bool hasRemoteNode) {
                    var nodes = await FindNode(Address, findCount, remote, hasRemoteNode, refreshKBucket: true).ConfigureAwait(true);
                    var lookupNodes = new List<Node>();
                    foreach (var node in nodes) {
                        if (node.Address == Address) continue;
                        lock (addresses) {
                            if (addresses.Count == findCount && (node.Address ^ Address) >= (addresses.Max ^ Address)) continue;
                            if (!addresses.Add(node.Address)) continue;
                            if (addresses.Count > findCount) addresses.Remove(addresses.Max);
                            lookupNodes.Add(node);
                        }
                    }

                    var findTasks = new List<Task>();
                    foreach (var node in lookupNodes) {
                        findTasks.Add(Find(node.IPEndPoint, false));
                    }
                    await findTasks.WhenAll();
                }

                async void RandomSearch() {
                    try {
                        for (int i = 0; i < 5; i++) {
                            var randomAddress = Address.Random();
                            var nodes = Nodes.FindNode(randomAddress, findCount);
                            var tasks = new Task[nodes.Length];
                            for (int j = 0; j < nodes.Length; j++) {
                                tasks[j] = FindNode(randomAddress, findCount, nodes[j].IPEndPoint, hasRemoteNode: false, refreshKBucket: true);
                            }
                            await tasks.WhenAll();
                        }
                    } catch { }
                }

                if (lookupTask is null) {
                    lookupTask = Find(seed, true).ContinueWith(delegate { RandomSearch(); });
                } else {
                    lookupTask = lookupTask.ContinueWith(delegate { Find(seed, true).ContinueWith(delegate { RandomSearch(); }).ConfigureAwait(true); });
                }
            }
        }



        [CommandHandler("ping")]
        private BDict? PingHandle(RemoteRequest r) {
            if (r.Request["ping"] is BAddress(Address myAddress) && myAddress == Address) {
                return new BDict { ["pong"] = r.Address };
            }
            return null;
        }

        [CommandHandler("find_node")]
        private BDict? FindNodeHandle(RemoteRequest r) {
            int findCount = Nodes.K;
            if (!(r.Request["target"] is BAddress(Address target))) return null;
            if (r.Request["count"] is BUInt(ulong count) && count > 0 && count < 50) findCount = (int)count;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(findCount * 39);
            try {
                int len = 0;
                foreach (var node in Nodes.FindNode(target, findCount)) {
                    len += WriteNode(buffer.AsSpan(len), node);
                }
                return new BDict { ["nodes"] = buffer[0..len] };
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
                _ = Nodes.Add(new Node(r.Address, r.Remote), lookup: false);
            }
        }

        [CommandHandler("get")]
        private BDict? GetHandle(RemoteRequest r) {
            if (ResponseGetValue is null) return null;

            int findCount = Nodes.K;
            if (!(r.Request["key"] is BBuffer(byte[] key))) return null;
            if (r.Request["count"] is BUInt(ulong count) && count > 0 && count < 50) findCount = (int)count;

            var eventArgs = new GetValueEventArgs(new Node(r.Address, r.Remote), key);
            ResponseGetValue(this, eventArgs);

            if (!eventArgs.HasValue) {
                Address target = HashTools.KeyToAddress(key);
                byte[] buffer = ArrayPool<byte>.Shared.Rent(findCount * 39);
                try {
                    int len = 0;
                    foreach (var node in Nodes.FindNode(target, findCount)) {
                        len += WriteNode(buffer.AsSpan(len), node);
                    }
                    return new BDict { ["nodes"] = buffer[0..len] };
                } finally {
                    ArrayPool<byte>.Shared.Return(buffer);
                    _ = Nodes.Add(new Node(r.Address, r.Remote), lookup: false);
                }
            } else {
                return new BDict { ["port"] = tcpServer.IPEndPoint.Port };
            }
        }

        [CommandHandler("broadcast")]
        private async Task BroadcastHandle(RemoteRequest r) {
            if (ReceiveBroadcast is null) return;

            if (!(r.Request["id"] is BAddress(Address id))) return;
            if (!(r.Request["i"] is BUInt(ulong ttl)) || ttl >= int.MaxValue) return;
            if (!(r.Request["msg"] is BBuffer(byte[] message))) return;

            lock (broadcastIdRecord) {
                if (!broadcastIdRecord.TryAdd(id, DateTime.Now)) return;
                if (broadcastIdRecord.Count > 1000) {
                    broadcastIdRecord.Remove(broadcastIdRecord.Keys.First());
                }
            }

            var eventArgs = new BroadcastEventArgs(new Node(r.Address, r.Remote), (int)ttl, message);
            var handlers = (EventHandler<BroadcastEventArgs>[])ReceiveBroadcast.GetInvocationList();
            foreach (var handler in handlers) {
                try {
                    eventArgs.Task = null;
                    handler(this, eventArgs);
                    if (eventArgs.IsCancelForward) return;
                    if (eventArgs.Task is { }) {
                        await eventArgs.Task;
                    }
                    if (eventArgs.IsCancelForward) return;
                } catch { }
            }

            Broadcast(message, id, (int)ttl + 1);
        }

        private static int WriteNode(Span<byte> buffer, Node node) {
            node.Address.WriteToBytes(buffer);

            var surviveTimeSpan = DateTime.Now - node.RefreshTime;
            if (surviveTimeSpan < TimeSpan.Zero) surviveTimeSpan = TimeSpan.Zero; else if (surviveTimeSpan > KBucket.Timeout) surviveTimeSpan = KBucket.Timeout;
            var surviveSeconds = surviveTimeSpan.TotalSeconds + (1L << 51) - (1L << 51); // 根据IEEE 754，此操作可以实现四舍五入并保留1bit小数
            byte surviveTime = (byte)(surviveSeconds * 2);

            IPAddress ip = node.IPEndPoint.Address;
            if (ip.IsIPv4MappedToIPv6) {
                ip = ip.MapToIPv4();
            }

            if (ip.AddressFamily is AddressFamily.InterNetworkV6) {
                surviveTime |= 0x80;
            }
            buffer[Address.Size] = surviveTime;
            ip.TryWriteBytes(buffer.Slice(Address.Size + 1), out var ipBytes);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(Address.Size + 1 + ipBytes, 2), (ushort)node.IPEndPoint.Port);
            return Address.Size + 3 + ipBytes;
        }

        private static int ReadNode(ReadOnlySpan<byte> buffer, out Node resultNode) {
            if (buffer.Length < 27) throw new ArgumentOutOfRangeException(nameof(buffer), "缓冲区太小");
            byte surviveTime = buffer[Address.Size];
            if (surviveTime >= 0x80 && buffer.Length < 39) throw new ArgumentOutOfRangeException(nameof(buffer), "缓冲区太小");

            var address = new Address(buffer.Slice(0, Address.Size));
            var surviveTimeSpan = TimeSpan.FromSeconds((surviveTime & 0x7f) / 2.0);
            var ipBytes = surviveTime >= 0x80 ? 16 : 4;
            var ip = new IPAddress(buffer.Slice(Address.Size + 1, ipBytes));
            var port = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(Address.Size + 1 + ipBytes, 2));
            resultNode = new Node(address, new IPEndPoint(ip, port), surviveTimeSpan);
            return Address.Size + 3 + ipBytes;
        }

        private async Task<bool> Ping(Node node) {
            try {
                var r = await udpServer.Request(new BDict { ["c"] = "ping", ["ping"] = node.Address }, node.IPEndPoint, cancellationToken: closeCancelTokenSource.Token);
                if (r.Address != node.Address) return false;
                if (!(r.Response["pong"] is BAddress(Address myAddress)) || myAddress != Address) return false;
                return true;
            } catch {
                return false;
            }
        }

        private async Task<Node[]> FindNode(Address target, int findCount, IPEndPoint remoteEP, bool hasRemoteNode, bool refreshKBucket = false, CancellationToken cancellationToken = default) {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, closeCancelTokenSource.Token);
            var r = await udpServer.Request(new BDict { ["c"] = "find_node", ["target"] = target, ["count"] = (ulong)findCount }, remoteEP, cancellationToken: tokenSource.Token);
            if (!(r.Response["nodes"] is BBuffer(byte[] buffer))) return Array.Empty<Node>();
            var nodes = new Dictionary<Address, Node>();
            for (int i = 0; i < buffer.Length;) {
                i += ReadNode(buffer.AsSpan(i), out var node);
                if (!nodes.ContainsKey(node.Address)) nodes.Add(node.Address, node);
            }
            if (hasRemoteNode) {
                nodes.Add(r.Address, new Node(r.Address, r.Remote));
            }

            if (refreshKBucket) {
                var tasks = new List<Task>();
                foreach (var node in nodes.Values) {
                    var task = Nodes.Add(node, lookup: true);
                    if (!task.IsCompleted) tasks.Add(task.AsTask());
                }
                await Task.WhenAll(tasks);
            }

            return nodes.Values.ToArray();
        }

        /// <summary>
        /// 返回Node表示找到值，返回Node[]表示未找到值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="findCount"></param>
        /// <param name="remoteEP"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<object> Get(byte[] key, int findCount, IPEndPoint remoteEP, CancellationToken cancellationToken = default) {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, closeCancelTokenSource.Token);
            var r = await udpServer.Request(new BDict { ["c"] = "get", ["key"] = key, ["count"] = (ulong)findCount }, remoteEP, cancellationToken: tokenSource.Token);

            if (r.Response["port"] is BInt(long port) && port > 0 && port < ushort.MaxValue) {
                return new Node(r.Address, new IPEndPoint(r.Remote.Address, (int)port));
            }

            if (r.Response["nodes"] is BBuffer(byte[] buffer)) {
                var nodes = new Dictionary<Address, Node>();
                for (int i = 0; i < buffer.Length;) {
                    i += ReadNode(buffer.AsSpan(i), out var node);
                    if (!nodes.ContainsKey(node.Address)) nodes.Add(node.Address, node);
                }

                return nodes.Values.ToArray();
            }
            return Array.Empty<Node>();
        }

        private void Broadcast(byte[] message, Hash<Size160> broadcastId, int ttl = 0) {
            if (ttl < 0 || ttl >= int.MaxValue) throw new ArgumentOutOfRangeException(nameof(ttl));

            var dict = new BDict { ["c"] = "broadcast", ["a"] = Address, ["msg"] = message, ["id"] = (Address)broadcastId, ["i"] = (ulong)ttl };
            var data = Bencode.Encode(dict, NetworkPrefix);
            var broadcastNodes = Nodes.FindNode(Address, Nodes.K, randomCount: 2);
            foreach (var node in broadcastNodes) {
                udpServer.Send(data, node.IPEndPoint);
            }
        }

        /// <summary>
        /// 在全网中查找目标地址的IP和端口
        /// </summary>
        /// <param name="target"></param>
        /// <param name="nodePoolSize"></param>
        /// <returns></returns>
        public async ValueTask<Node?> Lookup(Address target, int nodePoolSize = 20, CancellationToken cancellationToken = default) {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var addresses = new SortedSet<Address>(target.Comparer);
            var result = new TaskCompletionSource<Node?>();

            async Task Find(IPEndPoint remote, bool hasRemoteNode) {
                var nodes = await FindNode(target, Nodes.K, remote, hasRemoteNode, refreshKBucket: false, cancellationTokenSource!.Token);
                var lookupNodes = new List<Node>();
                foreach (var node in nodes) {
                    if (cancellationTokenSource.IsCancellationRequested) return;
                    if (node.Address == target) {
                        cancellationTokenSource.Cancel();
                        result!.TrySetResult(node);
                        return;
                    }
                    lock (addresses!) {
                        if (addresses.Count == nodePoolSize && (node.Address ^ target) >= (addresses.Max ^ target)) continue;
                        if (!addresses.Add(node.Address)) continue;
                        if (addresses.Count > nodePoolSize) addresses.Remove(addresses.Max);
                        lookupNodes.Add(node);
                    }
                }
                await lookupNodes.Select(node => Find(node.IPEndPoint, false)).WhenAll();
            }

            var tempNodes = Nodes.FindNode(target, Nodes.K);
            if (tempNodes.FirstOrDefault(n => n.Address == target) is Node r) return r;
            await Array.ConvertAll(tempNodes, node => Find(node.IPEndPoint, true)).WhenAll();
            result.TrySetResult(null);
            return await result.Task;
        }

        public ChannelReader<Node> Get(byte[] key, int nodePoolSize = 20, CancellationToken cancellationToken = default) {
            Address target = HashTools.KeyToAddress(key);
            var addresses = new SortedSet<Address>(target.Comparer);
            var result = Channel.CreateBounded<Node>(1);

            async Task Find(Node remoteNode) {
                if (cancellationToken.IsCancellationRequested) return;

                lock (addresses!) {
                    if (addresses.Count == nodePoolSize && (remoteNode.Address ^ target) >= (addresses.Max ^ target)) return;
                    if (!addresses.Add(remoteNode.Address)) return;
                    if (addresses.Count > nodePoolSize) addresses.Remove(addresses.Max);
                }

                var r = await Get(key, Nodes.K, remoteNode.IPEndPoint, cancellationToken);
                switch (r) {
                    case Node[] nodes:
                        await Array.ConvertAll(nodes, node => Find(node)).WhenAll();
                        break;
                    case Node node:
                        await result!.Writer.WriteAsync(node, cancellationToken);
                        break;
                }
            }

            var tempNodes = Nodes.FindNode(target, Nodes.K);
            _ = Array.ConvertAll(tempNodes, node => Find(node)).WhenAll().ContinueWith(delegate { result.Writer.TryComplete(); });
            return result;
        }


        public void Broadcast(byte[] message) {
            var broadcastId = Hash<Size160>.Random();
            lock (broadcastIdRecord) {
                if (!broadcastIdRecord.TryAdd(broadcastId, DateTime.Now)) return;
            }
            Broadcast(message, broadcastId);
        }





        public bool IsDisposed { get; private set; } = false;

        async ValueTask Dispose(bool disposing) {
            if (!IsDisposed) {
                closeCancelTokenSource.Cancel();
                refreshKBucketTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (disposing) {
                    await udpServer.DisposeAsync();
                    await refreshKBucketTimer.DisposeAsync();
                    GC.SuppressFinalize(this);
                }
                IsDisposed = true;
            }
        }

        ~Client() {
            Dispose(false).AsTask().Wait();
        }

        public ValueTask DisposeAsync() => Dispose(true);
    }
}
