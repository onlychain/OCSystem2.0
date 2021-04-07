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
using System.Runtime.CompilerServices;

namespace OnlyChain.Network {
    [System.Diagnostics.DebuggerDisplay("{Address}, {EndPoint}")]
    public sealed class Client : IClient, IAsyncDisposable {
        readonly TimeSpan refreshKBucketTimeSpan = TimeSpan.FromSeconds(20);
        static readonly TimeSpan broadcastTimeout = TimeSpan.FromHours(2);

        private readonly Timer refreshKBucketTimer;
        private readonly CancellationTokenSource closeCancelTokenSource = new CancellationTokenSource();
        private readonly Dictionary<Bytes<Hash160>, DateTime> broadcastIdRecord = new Dictionary<Bytes<Hash160>, DateTime>();
        private readonly Task initializationTask;

        public string? Name { get; }
        public string? NetworkPrefix { get; }
        public Bytes<Address> Address { get; }
        public IPEndPoint EndPoint { get; }
        public IPEndPoint BindEndPoint { get; }
        public KBucket Nodes { get; }
        public CancellationToken CloseCancellationToken => closeCancelTokenSource.Token;
        public BlockChainSystem System { get; }
        public ProducerSystem? ProducerSystem { get; }
        public P2P P2P { get; }
        public SuperNodeConfig? SuperConfig { get; }
        public bool CanBroadcastBlock { get; private set; } = false;

        internal Task InitializationTask => initializationTask;


        public Client(Bytes<Address> address, IPEndPoint endPoint, string baseDirectory, string? networkPrefix = null, IEnumerable<IPEndPoint>? seeds = null, SuperNodeConfig? superConfig = null, string? name = null) {
            Name = name ?? endPoint.ToString();
            Address = address;
            NetworkPrefix = networkPrefix;
            SuperConfig = superConfig;
            EndPoint = endPoint;
            BindEndPoint = new IPEndPoint(IPAddress.Any, endPoint.Port);
            string chainName = networkPrefix ?? "main";
            System = new BlockChainSystem(this, Path.Combine(baseDirectory, $"{address}-{chainName}"), chainName);
            P2P = new P2P(this);
            if (superConfig is not null) ProducerSystem = new ProducerSystem(this);
            Nodes = new KBucket(16, address, P2P.Ping);

            P2P.Init();

            Random random = new Random();
            refreshKBucketTimeSpan = TimeSpan.FromSeconds(random.NextDouble() * 5 + 60);
            refreshKBucketTimer = new Timer(async delegate {
                if (closeCancelTokenSource.IsCancellationRequested) return;
                var randomAddress = Bytes<Address>.Random();
                var neighborNodes = Nodes.FindNode(randomAddress, 10);
                var tasks = Array.ConvertAll(neighborNodes, node => P2P.FindNode(randomAddress, Nodes.K, node.IPEndPoint, hasRemoteNode: false, refreshKBucket: true));

                var now = DateTime.UtcNow;
                lock (broadcastIdRecord) {
                    var removeKeys = new List<Bytes<Hash160>>();
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
            var addresses = new SortedSet<Bytes<Address>>(Comparer<Bytes<Address>>.Create((a, b) => (a ^ Address).CompareTo(b ^ Address)));
            foreach (var seed in seeds ?? Enumerable.Empty<IPEndPoint>()) {
                const int findCount = 10;

                async Task Find(IPEndPoint remote, bool hasRemoteNode) {
                    try {
                        //Console.WriteLine($"{Address} => find_node: {remote}");
                        var nodes = await P2P.FindNode(Address, findCount, remote, hasRemoteNode, refreshKBucket: true).ConfigureAwait(true);
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
                    } catch { }
                }

                async void RandomSearch() {
                    // Console.WriteLine($"RandomSearch: {Address}");
                    try {
                        for (int i = 0; i < 5; i++) {
                            var randomAddress = Bytes<Address>.Random();
                            var nodes = Nodes.FindNode(randomAddress, findCount);
                            var tasks = new Task[nodes.Length];
                            for (int j = 0; j < nodes.Length; j++) {
                                tasks[j] = P2P.FindNode(randomAddress, findCount, nodes[j].IPEndPoint, hasRemoteNode: false, refreshKBucket: true);
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

            initializationTask = lookupTask ?? Task.CompletedTask;
        }

        public async Task Initialization() {

            await Task.WhenAll(System.LoadDatabaseTask, initializationTask);
            await System.DownloadBlocks();
            CanBroadcastBlock = true;

            ProducerSystem?.Start();
        }


        public Task<Node?> GetTargetNode(Bytes<Address> address) {
            if (address == Address) Task.FromResult<Node?>(new Node(address, EndPoint));

            if (Nodes.TryGetValue(address, out Node? node)) {
                return Task.FromResult<Node?>(node);
            }
            return P2P.GetTargetNode(address);
        }

        public async Task<Block?> DownloadBlock(uint height) {
            BDict args = new BDict { ["cmd"] = "get_block", ["height"] = height };
            await foreach (var result in P2P.GetValue(args, cancellationToken: CloseCancellationToken)) {
                if (result.Node is null) continue;
                try {
                    if (await P2P.GetData(result.Node.IPEndPoint, args) is not byte[] data) continue;
                    return new Block(data);
                } catch {

                }
            }

            return null;
        }



        public void Log(object message) {
            if (Address == "85e23a0c02ed19be939aa9be4499322467cebd17") {
                Console.WriteLine(message);
            }
        }

        // 1. 从本地数据库按高度顺序读取区块，并更新区块链系统状态
        // 2. 随机选取节点，获得当前区块最大高度（此高度可能不可信），并依次从这些节点拉取区块，当高度>=最大高度或全部节点遍历完毕则结束该过程
        // 3. 判断本节点是否为超级节点，是则启动生产系统


        public bool IsDisposed { get; private set; } = false;


        async ValueTask Dispose(bool disposing) {
            if (!IsDisposed) {
                closeCancelTokenSource.Cancel();
                refreshKBucketTimer.Change(Timeout.Infinite, Timeout.Infinite);
                ProducerSystem?.Stop();
                if (disposing) {
                    await P2P.DisposeAsync().ConfigureAwait(false);
                    await refreshKBucketTimer.DisposeAsync().ConfigureAwait(false);
                    System.Dispose();
                    ProducerSystem?.Dispose();
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
