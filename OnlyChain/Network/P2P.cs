#nullable enable

using OnlyChain.Core;
using OnlyChain.Model;
using OnlyChain.Network.Objects;
using OnlyChain.Secp256k1;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // 命名样式

namespace OnlyChain.Network {
    public sealed class P2P : IAsyncDisposable {
        delegate bool GetValueDelegate(GetValueEventArgs e);

        private readonly Client client;
        private readonly UdpServer udpServer;
        private readonly TcpServer tcpServer;
        private readonly Dictionary<Bytes<Hash160>, DateTime> broadcastIdRecord = new();
        private readonly Dictionary<string, Action<BroadcastEventArgs>> broadcastHandlers = new();
        private readonly Dictionary<string, Func<BDict, BObject?>> getSmallValueHandlers = new();
        private readonly Dictionary<string, GetValueDelegate> getValueHandlers = new();
        private readonly Dictionary<string, Func<NetworkStream, BDict, ValueTask>> getMultiValueHandlers = new();
        private readonly Dictionary<string, Func<SuperEventArgs, ValueTask>> superHandlers = new();


        public event EventHandler<BroadcastEventArgs> ReceiveBroadcast = null!;
        public event EventHandler<SuperConnectEventArgs> SuperConnected = null!;

        public P2P(Client client) {
            this.client = client;
            udpServer = new UdpServer(client);
            tcpServer = new TcpServer(client);
        }

        public void Init() {
            BindHandlers(broadcastHandlers);
            BindHandlers(getSmallValueHandlers);
            BindHandlers(getValueHandlers);
            BindHandlers(getMultiValueHandlers);
            BindHandlers(superHandlers);

            udpServer.Start();
        }

        /***************
        请求: dict, data stream
        响应: dict, data stream

        */
        internal async ValueTask TcpServer_GetValue(NetworkStream stream, BDict args) {
            string cmd = (string)args["cmd"];
            if (cmd is "super") {
                await HandleSuperNode(stream, args);
            } else if (getSmallValueHandlers.TryGetValue(cmd, out var getSmallHandler)) {
                await Bencode.EncodeNoPrefixAsync(stream, new BDict { ["result"] = getSmallHandler(args) }, client.CloseCancellationToken);
            } else if (getValueHandlers.TryGetValue(cmd, out var handler)) {
                var e = new GetValueEventArgs(args, stream);
                if (handler(e)) {
                    Debug.Assert(e.Value is not null);
                    await Bencode.EncodeNoPrefixAsync(stream, new BDict { ["result"] = true, ["length"] = e.Value.Length }, client.CloseCancellationToken);
                    await stream.WriteAsync(e.Value, client.CloseCancellationToken);
                } else {
                    await Bencode.EncodeNoPrefixAsync(stream, new BDict { ["result"] = false }, client.CloseCancellationToken);
                }
            } else if (getMultiValueHandlers.TryGetValue(cmd, out var getMultiValueHandler)) {
                await getMultiValueHandler(stream, args);
            }
        }

        private void BindHandlers<TDelegate>(IDictionary<string, TDelegate> handlers) where TDelegate : Delegate {
            MethodInfo targetMethod = typeof(TDelegate).GetMethod("Invoke")!;
            Type[] targetParams = Array.ConvertAll(targetMethod.GetParameters(), p => p.ParameterType);
            Type type = GetType();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
                if (method.GetCustomAttribute<CommandHandlerAttribute>() is not { Command: string cmd }) continue;

                var paramTypes = Array.ConvertAll(method.GetParameters(), p => p.ParameterType);
                if (paramTypes.Length != targetParams.Length) continue;
                for (int i = 0; i < paramTypes.Length; i++) {
                    if (paramTypes[i] != targetParams[i]) goto Continue;
                }

                if (method.ReturnType == targetMethod.ReturnType) {
                    var handler = (TDelegate)method.CreateDelegate(typeof(TDelegate), this);
                    handlers.Add(cmd, handler);
                }

            Continue:;
            }
        }

        private static int WriteNode(Span<byte> buffer, Node node) {
            node.Address.CopyTo(buffer);

            var surviveTimeSpan = DateTime.UtcNow - node.RefreshTime;
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
            buffer[Bytes<Address>.Size] = surviveTime;
            ip.TryWriteBytes(buffer[(Bytes<Address>.Size + 1)..], out var ipBytes);
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(Bytes<Address>.Size + 1 + ipBytes, 2), (ushort)node.IPEndPoint.Port);
            return Bytes<Address>.Size + 3 + ipBytes;
        }

        private static int ReadNode(ReadOnlySpan<byte> buffer, out Node resultNode) {
            if (buffer.Length < 27) throw new ArgumentOutOfRangeException(nameof(buffer), "缓冲区太小");
            byte surviveTime = buffer[Bytes<Address>.Size];
            if (surviveTime >= 0x80 && buffer.Length < 39) throw new ArgumentOutOfRangeException(nameof(buffer), "缓冲区太小");

            var address = new Bytes<Address>(buffer.Slice(0, Bytes<Address>.Size));
            var surviveTimeSpan = TimeSpan.FromSeconds((surviveTime & 0x7f) / 2.0);
            var ipBytes = surviveTime >= 0x80 ? 16 : 4;
            var ip = new IPAddress(buffer.Slice(Bytes<Address>.Size + 1, ipBytes));
            var port = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(Bytes<Address>.Size + 1 + ipBytes, 2));
            resultNode = new Node(address, new IPEndPoint(ip, port), surviveTimeSpan);
            return Bytes<Address>.Size + 3 + ipBytes;
        }


        #region ping

        [CommandHandler("ping")]

        private BDict? __P2P_Ping(RemoteRequest r) {
            if (r.Request["ping"] is BAddress(Bytes<Address> myAddress) && myAddress == client.Address) {
                return new BDict { ["pong"] = r.Address };
            }
            return null;
        }

        public async Task<bool> Ping(Node node) {
            try {
                var r = await udpServer.Request(new BDict { ["c"] = "ping", ["ping"] = node.Address }, node.IPEndPoint, cancellationToken: client.CloseCancellationToken);
                if (r.Address != node.Address) return false;
                if (!(r.Response["pong"] is BAddress(Bytes<Address> myAddress)) || myAddress != client.Address) return false;
                return true;
            } catch {
                return false;
            }
        }

        #endregion


        #region find_node

        [CommandHandler("find_node")]
        private BDict? __P2P_FindNode(RemoteRequest r) {
            int findCount = client.Nodes.K;
            if (!(r.Request["target"] is BAddress(Bytes<Address> target))) return null;
            if (r.Request["count"] is BUInt(ulong count) && count > 0 && count < 50) findCount = (int)count;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(findCount * 39);
            try {
                int len = 0;
                foreach (var node in client.Nodes.FindNode(target, findCount)) {
                    len += WriteNode(buffer.AsSpan(len), node);
                }
                return new BDict { ["nodes"] = buffer[0..len] };
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
                _ = client.Nodes.Add(new Node(r.Address, r.Remote), lookup: false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="findCount"></param>
        /// <param name="remoteEP"></param>
        /// <param name="hasRemoteNode"></param>
        /// <param name="refreshKBucket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Node[]> FindNode(Bytes<Address> target, int findCount, IPEndPoint remoteEP, bool hasRemoteNode, bool refreshKBucket = false, CancellationToken cancellationToken = default) {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken);
            var r = await udpServer.Request(new BDict { ["c"] = "find_node", ["target"] = target, ["count"] = (ulong)findCount }, remoteEP, cancellationToken: tokenSource.Token);
            if (!(r.Response["nodes"] is BBuffer(byte[] buffer))) return Array.Empty<Node>();
            var nodes = new Dictionary<Bytes<Address>, Node>();
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
                    var task = client.Nodes.Add(node, lookup: true);
                    if (!task.IsCompleted) tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }

            return nodes.Values.ToArray();
        }

        public async Task<Node?> GetTargetNode(Bytes<Address> targetAddress, int nodePoolSize = 20, CancellationToken cancellationToken = default) {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken);

            var addresses = new SortedSet<Bytes<Address>>(targetAddress.Comparer);
            var queue = Channel.CreateUnbounded<Node>();

            bool CanFind(Node node) {
                lock (addresses!) {
                    if (addresses.Count == nodePoolSize && (node.Address ^ targetAddress) >= (addresses.Max ^ targetAddress)) return false;
                    if (addresses.Add(node.Address) is false) return false;
                    if (addresses.Count > nodePoolSize) addresses.Remove(addresses.Max);
                }
                return true;
            }

            var tempNodes = client.Nodes.FindNode(targetAddress, client.Nodes.K);
            if (tempNodes.FirstOrDefault(n => n.Address == targetAddress) is Node target) return target;
            int nodeCount = 0;
            foreach (var n in tempNodes) {
                if (CanFind(n) is false) continue;
                nodeCount++;
                await queue.Writer.WriteAsync(n, cancellationTokenSource.Token);
            }

            if (nodeCount == 0) {
                queue.Writer.Complete();
                return null;
            }

            var result = new TaskCompletionSource<Node?>();
            const int Concurrency = 8;
            int forCount = 0;
            for (int i = 0; i < Concurrency; i++) {
                _ = Task.Run(async delegate {
                    try {
                        await foreach (var node in queue.Reader.ReadAllAsync(cancellationTokenSource.Token)) {
                            Interlocked.Increment(ref forCount);
                            try {
                                Node[] nodes = await FindNode(targetAddress, client.Nodes.K, node.IPEndPoint, hasRemoteNode: false, cancellationToken: cancellationTokenSource.Token);
                                if (nodes.FirstOrDefault(n => n.Address == targetAddress) is Node target) {
                                    queue.Writer.Complete();
                                    result.TrySetResult(target);
                                    return;
                                }

                                foreach (var n in nodes) {
                                    if (CanFind(n) is false) continue;
                                    Interlocked.Increment(ref nodeCount);
                                    await queue.Writer.WriteAsync(n, cancellationTokenSource.Token);
                                }
                            } catch (TaskCanceledException) {

                            } finally {
                                if (Interlocked.Decrement(ref nodeCount) == 0) {
                                    queue.Writer.TryComplete();
                                    result.TrySetResult(null);
                                }
                            }
                        }
                    } catch {
                    }
                }, cancellationTokenSource.Token);
            }

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            try {
                return await result.Task;
            } finally {
                sw.Stop();
                cancellationTokenSource.Cancel();
                //Console.WriteLine($"GetTargetNode: {sw.Elapsed}, ForCount: {forCount}");
            }
        }

        #endregion


        #region get

        [CommandHandler("get")]

        private BDict? __P2P_Get(RemoteRequest r) {
            int findCount = client.Nodes.K;
            if (r.Request["key"] is not BDict key) return null;
            if (r.Request["count"] is BUInt(ulong count) && count > 0 && count < 50) findCount = (int)count;

            string cmd = (string)key["cmd"];
            BObject? smallValue = null;
            bool hasSmallValue = false;
            bool hasValue = false;
            if (getSmallValueHandlers.TryGetValue(cmd, out var getSmallValueHandler)) {
                smallValue = getSmallValueHandler(key);
                hasSmallValue = true;
            } else if (getValueHandlers.TryGetValue(cmd, out var getValueHandler)) {
                hasValue = getValueHandler(new GetValueEventArgs(key, stream: null));
            } else {
                return null;
            }

            Bytes<Address> target = HashTools.ToHash160(key).ToBytes<Address>();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(findCount * 39);
            try {
                int len = 0;
                foreach (var node in client.Nodes.FindNode(target, findCount)) {
                    len += WriteNode(buffer.AsSpan(len), node);
                }
                BObject result;
                if (hasSmallValue) {
                    if (smallValue is not null) {
                        result = smallValue;
                    } else {
                        result = false;
                    }
                } else {
                    result = hasValue;
                }
                return new BDict { ["nodes"] = buffer[0..len], ["result"] = result };
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
                _ = client.Nodes.Add(new Node(r.Address, r.Remote), lookup: false);
            }
        }

        public async Task<GetValueResult> Get(BDict key, int findCount, IPEndPoint remoteEP, CancellationToken cancellationToken = default) {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken);
            var r = await udpServer.Request(new BDict { ["c"] = "get", ["key"] = key, ["count"] = (ulong)findCount }, remoteEP, cancellationToken: tokenSource.Token);

            Node[] resultNodes = Array.Empty<Node>();
            if (r.Response["nodes"] is BBuffer(byte[] buffer)) {
                var nodes = new Dictionary<Bytes<Address>, Node>();
                for (int i = 0; i < buffer.Length;) {
                    i += ReadNode(buffer.AsSpan(i), out var node);
                    if (!nodes.ContainsKey(node.Address)) nodes.Add(node.Address, node);
                }

                resultNodes = nodes.Values.ToArray();
            }

            return r.Response["result"] switch {
                BBool(true) => new GetValueResult(resultNodes, new Node(r.Address, r.Remote), null),
                BBool(false) or null => new GetValueResult(resultNodes, null, null),
                BObject obj => new GetValueResult(resultNodes, new Node(r.Address, r.Remote), obj),
            };
        }

        public async IAsyncEnumerable<ValueResult> GetValue(BDict key, int nodePoolSize = 10, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken).Token;

            Bytes<Address> target = HashTools.ToHash160(key).ToBytes<Address>();
            var addresses = new SortedSet<Bytes<Address>>(target.Comparer);
            var queue = Channel.CreateUnbounded<Node>();

            var tempNodes = client.Nodes.FindNode(target, client.Nodes.K);
            int nodeCount = tempNodes.Length;
            foreach (var n in tempNodes) {
                await queue.Writer.WriteAsync(n, cancellationToken);
            }

            if (nodeCount == 0) {
                queue.Writer.Complete();
                yield break;
            }

            var result = Channel.CreateBounded<ValueResult>(1);
            const int Concurrency = 3;
            for (int i = 0; i < Concurrency; i++) {
                _ = Task.Run(async delegate {
                    try {
                        await foreach (var node in queue.Reader.ReadAllAsync(cancellationToken)) {
                            try {
                                lock (addresses!) {
                                    if (addresses.Count == nodePoolSize && (node.Address ^ target) >= (addresses.Max ^ target)) continue;
                                    if (addresses.Add(node.Address) is false) continue;
                                    if (addresses.Count > nodePoolSize) addresses.Remove(addresses.Max);
                                }

                                var r = await Get(key, client.Nodes.K, node.IPEndPoint, cancellationToken);
                                if (r.Result.HasValue) {
                                    await result.Writer.WriteAsync(r.Result, cancellationToken);
                                }

                                foreach (var n in r.Nodes) {
                                    Interlocked.Increment(ref nodeCount);
                                    await queue.Writer.WriteAsync(n, cancellationToken);
                                }
                            } catch {

                            } finally {
                                if (Interlocked.Decrement(ref nodeCount) == 0) {
                                    queue.Writer.Complete();
                                    result.Writer.Complete();
                                }
                            }
                        }
                    } catch { }
                }, cancellationToken);
            }

            await foreach (var n in result.Reader.ReadAllAsync(cancellationToken)) {
                yield return n;
            }
        }

        #endregion


        #region broadcast

        private bool AddBroadcastId(Bytes<Hash160> id) {
            lock (broadcastIdRecord) {
                if (!broadcastIdRecord.TryAdd(id, DateTime.UtcNow)) return false;
                if (broadcastIdRecord.Count > 1000) {
                    broadcastIdRecord.Remove(broadcastIdRecord.Keys.First());
                }
            }
            return true;
        }

        [CommandHandler("broadcast")]
        private async void __P2P_Broadcast(RemoteRequest r) {
            if (r.Request["id"] is not BAddress(Bytes<Address> id)) return;
            if (!(r.Request["i"] is BUInt(ulong ttl)) || ttl >= int.MaxValue) return;
            if (r.Request["cmd"] is not BString(string cmd)) return;
            if (r.Request["msg"] is not BDict message) return;

            if (AddBroadcastId(id.ToBytes<Hash160>()) is false) return;

            var eventArgs = new BroadcastEventArgs(new Node(r.Address, r.Remote), (int)ttl, cmd, message);
            if (broadcastHandlers.TryGetValue(cmd, out var handler)) {
                try {
                    handler(eventArgs);
                    if (eventArgs.IsCancelForward) return;
                    if (eventArgs.Task is not null) await eventArgs.Task;
                    if (eventArgs.IsCancelForward) return;
                } catch { return; }
            } else {
                await OnReceiveBroadcast(eventArgs);
            }


            if (eventArgs.IsCancelForward) return;
            Broadcast(cmd, message, id.ToBytes<Hash160>(), (int)ttl + 1);
        }

        public void Broadcast(string cmd, BDict message, Bytes<Hash160> broadcastId, int ttl = 0) {
            if (ttl < 0 || ttl >= int.MaxValue) throw new ArgumentOutOfRangeException(nameof(ttl));

            var dict = new BDict {
                ["c"] = "broadcast",
                ["a"] = client.Address,
                ["msg"] = message,
                ["id"] = broadcastId.ToBytes<Address>(),
                ["i"] = (ulong)ttl,
                ["cmd"] = cmd,
            };
            var data = Bencode.Encode(dict, client.NetworkPrefix);
            var broadcastNodes = client.Nodes.FindNode(client.Address, count: 4, randomCount: 2);
            foreach (var node in broadcastNodes) {
                udpServer.Send(data, node.IPEndPoint);
            }
        }

        private async Task OnReceiveBroadcast(BroadcastEventArgs e) {
            var handlers = (EventHandler<BroadcastEventArgs>[])ReceiveBroadcast.GetInvocationList();
            foreach (var handler in handlers) {
                try {
                    e.Task = null;
                    handler(this, e);
                    if (e.IsCancelForward) return;
                    if (e.Task is not null) {
                        await e.Task;
                    }
                } catch {
                    e.CancelForward();
                }
                if (e.IsCancelForward) return;
            }
        }

        private void Broadcast(string cmd, BDict msg) {
            Bytes<Hash160> broadcastId = Bytes<Hash160>.Random();
            if (AddBroadcastId(broadcastId) is false) return;
            Broadcast(cmd, msg, broadcastId);
        }

        #endregion


        #region broadcast handlers

        readonly static Exception CancelBroadcast = new Exception();

        /// <summary>
        /// 竞选节点登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [CommandHandler("login")]
        private void __Broadcast_Login(BroadcastEventArgs e) {
            var m = e.Message;
            if (Ecdsa.Verify(m) is false) throw CancelBroadcast;

            var ep = IPEndPoint.Parse((string)m["ep"]);
            var time = BlockChainTimestamp.ToDateTime((uint)m["time"]);
            var publicKey = Ecdsa.GetPublicKey(m);
            var addr = publicKey.ToAddress();
            var randBytes = (byte[])m["nonce"];

            if (DateTime.UtcNow - time >= TimeSpan.FromHours(1) || time - DateTime.UtcNow >= TimeSpan.FromMinutes(5)) throw CancelBroadcast;
            if (randBytes is not { Length: 32 }) throw CancelBroadcast;

            if (!client.System.CampaignNodes.TryGetValue(addr, out SuperPeer? oldSuperPeer)) throw CancelBroadcast; // 非竞选节点，丢弃并阻断广播

            SuperPeer superPeer;
            if (ep.Equals(oldSuperPeer?.IPEndPoint)) { // IP 端口与本地保存的一致
                superPeer = oldSuperPeer;
            } else {
                superPeer = new SuperPeer(publicKey, ep);
                client.System.CampaignNodes[addr] = superPeer;
            }

            if (client.System.IsProducer(client.Address) && client.System.IsProducer(addr) && !superPeer.IsOnline) {
                e.Task = SuperConnect(superPeer);
            }
        }

        public void BroadcastLogin() {
            if (client.SuperConfig?.PrivateKey is null) throw new InvalidOperationException("要求具有私钥才能调用此方法");

            BDict msg = new BDict {
                ["ep"] = $"{client.EndPoint}",
                ["time"] = BlockChainTimestamp.NowTimestamp,
                ["nonce"] = Bytes<Hash256>.Random().ToArray(),
            };
            Ecdsa.Sign(msg, client.SuperConfig.PrivateKey);
            Broadcast("login", msg);
        }


        [CommandHandler("new_block")]
        private void __Broadcast_NewBlock(BroadcastEventArgs e) {
            if (client.CanBroadcastBlock is false) throw CancelBroadcast;

            var blockHeader = new Block((byte[])e.Message["header"], hasTx: false);
            if (blockHeader.Height <= client.System.LastBlock.Height) throw CancelBroadcast;
            if (!Ecdsa.Verify(blockHeader.ProducerPublicKey, blockHeader.HashSignHeader, blockHeader.Signature)) throw CancelBroadcast;
            e.Task = Task.Run(async delegate {
                await client.System.DownloadBlocks(blockHeader.Height, remote: e.Sender.IPEndPoint);
            });
        }

        public void BroadcastBlock(Block block) {
            BDict msg = new BDict {
                ["header"] = block.SerializeHeader(),
            };
            Broadcast("new_block", msg);
        }




        #endregion


        #region super handlers

        private async ValueTask HandleSuperNode(NetworkStream stream, BDict args) {
            // 1、发送随机串
            // 2、接收认证头部
            // 3、进行认证

            byte[] nonce = Bytes<U256>.Random().ToArray();
            await Bencode.EncodeNoPrefixAsync(stream, new BDict {
                ["nonce1"] = nonce
            }, client.CloseCancellationToken);


            var d = (BDict)await Bencode.DecodeNoPrefixAsync(stream, cancellationToken: client.CloseCancellationToken);
            if (d is not { Count: 5 }) return;
            if (((byte[])d["nonce1"]).AsSpan().SequenceEqual(nonce) is false) return;
            if (d["nonce2"] is not BBuffer({ Length: 32 })) return;
            if (IPEndPoint.TryParse((string)d["ep"], out IPEndPoint? remoteEP) is false || remoteEP is null) return;
            if (Ecdsa.Verify(d) is false) return;

            PublicKey publicKey = Ecdsa.GetPublicKey(d);
            var remote = new SuperNode(publicKey, remoteEP);
            if (client.System.IsProducer(remote.Address) is false) return;

            await Bencode.EncodeNoPrefixAsync(stream, new BDict { ["ok"] = true }, cancellationToken: client.CloseCancellationToken);

            SuperConnected?.Invoke(this, new SuperConnectEventArgs(remote));

            stream.ReadTimeout = -1;
            stream.WriteTimeout = -1;

            try {
                while (client.CloseCancellationToken.IsCancellationRequested is false) {
                    d = (BDict)await Bencode.DecodeNoPrefixAsync(stream, cancellationToken: client.CloseCancellationToken);
                    if (client.System.IsProducer(remote.Address) is false) return;
                    string cmd = (string)d["cmd"];
                    await superHandlers[cmd](new SuperEventArgs(stream, d, remote));
                }
            } catch (OperationCanceledException) {

            } catch {
                // Console.WriteLine($"ERROR: {client.Address}{Environment.NewLine}{ex}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public async Task<bool> SuperConnect(SuperPeer peer) {
            if (client.SuperConfig?.PrivateKey is null) throw new InvalidOperationException("要求具有私钥才能调用此方法");

            if (peer.IsOnline) return true;

            try {
                await peer.InitSocket();

                using var stream = new NetworkStream(peer.Socket!, ownsSocket: false);

                await Bencode.EncodeAsync(stream, new BDict { ["cmd"] = "super" }, client.NetworkPrefix, cancellationToken: client.CloseCancellationToken);

                BDict d = (BDict)await Bencode.DecodeNoPrefixAsync(stream, cancellationToken: client.CloseCancellationToken);
                byte[] nonce1 = (byte[])d["nonce1"];

                d = new BDict {
                    ["nonce1"] = nonce1,
                    ["nonce2"] = Bytes<U256>.Random().ToArray(),
                    ["ep"] = $"{client.EndPoint}",
                };
                Ecdsa.Sign(d, client.SuperConfig.PrivateKey);
                await Bencode.EncodeNoPrefixAsync(stream, d, cancellationToken: client.CloseCancellationToken);

                d = (BDict)await Bencode.DecodeNoPrefixAsync(stream, cancellationToken: client.CloseCancellationToken);
                return (bool)d["ok"];
            } catch {
                return false;
            }
        }

        public async Task SuperSend(SuperPeer peer, BDict headerDict, ReadOnlyMemory<byte> body) {
            if (!await SuperConnect(peer)) throw new InvalidOperationException("peer连接失败");

            using var stream = new NetworkStream(peer.Socket!, ownsSocket: false);

            await peer.SendLock!.WaitAsync(client.CloseCancellationToken);
            try {
                await Bencode.EncodeNoPrefixAsync(stream, headerDict, client.CloseCancellationToken);
                await stream.WriteAsync(body, client.CloseCancellationToken);
            } finally {
                peer.SendLock.Release();
            }
        }


        private static async ValueTask<byte[]> ReadDataAsync(SuperEventArgs e, CancellationToken cancellationToken = default) {
            int dataLength = (int)e.Dict["length"];
            return await e.Stream.ReadBytesAsync(dataLength, cancellationToken);
        }

        [CommandHandler("producer_chip")]
        private async ValueTask __Super_ProducerBlockChip(SuperEventArgs e) {
            client.ProducerSystem?.ProducerBlockChipArrived(e, await ReadDataAsync(e, client.CloseCancellationToken));
        }

        [CommandHandler("other_chip")]
        private async ValueTask __Super_OtherBlockChip(SuperEventArgs e) {
            client.ProducerSystem?.OtherBlockChipArrived(e, await ReadDataAsync(e, client.CloseCancellationToken));
        }

        [CommandHandler("precommit_vote")]
        private async ValueTask __Super_PrecommitVote(SuperEventArgs e) {
            client.ProducerSystem?.PrecommitVoteArrived(e, await ReadDataAsync(e, client.CloseCancellationToken));
        }

        [CommandHandler("commit_vote")]
        private async ValueTask __Super_CommitVote(SuperEventArgs e) {
            client.ProducerSystem?.CommitVoteArrived(e, await ReadDataAsync(e, client.CloseCancellationToken));
        }


        #endregion


        private async ValueTask<NetworkStream> GetNetworkStream(IPEndPoint remote, CancellationToken cancellationToken = default) {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken);

            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(client.EndPoint.Address, 0));
            await socket.ConnectAsync(remote.Address, remote.Port, cancellationToken: cancellationTokenSource.Token);
            return new NetworkStream(socket, ownsSocket: true);
        }

        public async ValueTask<byte[]?> GetData(IPEndPoint remote, BDict args, ReadOnlyMemory<byte> postData = default) {
            try {
                using var stream = await GetNetworkStream(remote);

                await Bencode.EncodeAsync(stream, args, client.NetworkPrefix, cancellationToken: client.CloseCancellationToken);
                await stream.WriteAsync(postData, cancellationToken: client.CloseCancellationToken);

                BDict r = (BDict)await Bencode.DecodeNoPrefixAsync(stream, cancellationToken: client.CloseCancellationToken);
                if ((bool)r["result"]) {
                    int length = (int)r["length"];
                    return await stream.ReadBytesAsync(length, cancellationToken: client.CloseCancellationToken);
                }
            } catch {
            }
            return null;
        }

        #region udp get value handlers

        [CommandHandler("block_height")]
        private BObject? __GetSmallValue_BlockHeight(BDict d) {
            return client.System.LastBlock.Height;
        }

        public async Task<uint> GetBlockHeight(IPEndPoint remote) {
            var r = await Get(new BDict { ["cmd"] = "block_height" }, client.Nodes.K, remote);
            if (r.Result.Value is BUInt(ulong height and < uint.MaxValue)) {
                return (uint)height;
            }
            return 0;
        }

        public async Task<uint> GetBlockHeight(IEnumerable<IPEndPoint> remotes) {
            uint result = 0;
            var tasks = new List<Task>();
            foreach (var ep in remotes) {
                tasks.Add(GetBlockHeight(ep).ContinueWith(t => {
                    if (t.IsCompletedSuccessfully) {
                        lock (tasks) {
                            result = Math.Max(result, t.Result);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            return result;
        }

        [CommandHandler("block_hash")]
        private BObject? __GetSmallValue_BlockHash(BDict d) {
            uint height = (uint)d["height"];
            byte[] hash = client.System.DB.GetBlockHash(height).ToArray();
            return hash;
        }

        public async Task<Bytes<Hash256>> GetBlockHash(IPEndPoint remote, uint height) {
            var r = await Get(new BDict { ["cmd"] = "block_hash", ["height"] = height }, client.Nodes.K, remote);
            if (r.Result.Value is BBuffer({ Length: 32 } hashBytes)) {
                return new Bytes<Hash256>(hashBytes);
            }
            return Bytes<Hash256>.Empty;
        }

        #endregion


        #region tcp get value handlers

        [CommandHandler("get_block")]
        private bool __GetValue_GetBlock(GetValueEventArgs e) {
            Bytes<Hash256> hash;
            try {
                hash = new Bytes<Hash256>((byte[])e.Dict["hash"]);
            } catch {
                uint height = (uint)e.Dict["height"];
                if (height < 1 || height > client.System.LastBlock.Height) goto NotFound;
                hash = client.System.DB.GetBlockHash(height);
            }

            if (e.IsGetValue) {
                if (e.Dict["only_header"] is BBool(true)) {
                    if (client.System.DB.GetBlockHeader(hash) is not byte[] header) goto NotFound;
                    e.Value = header;
                } else {
                    if (client.System.DB.GetBlock(hash) is not Block block) goto NotFound;
                    e.Value = block.NetworkSerialize();
                }
            } else {
                if (client.System.DB.GetBlockHeader(hash) is null) goto NotFound;
            }
            return true;

        NotFound:
            return false;
        }

        private async Task<Block?> GetBlock(IPEndPoint remote, BDict args) {
            return new Block(await GetData(remote, args), hasTx: args["only_header"] is not BBool(true));
        }

        public async Task<Block?> GetBlockFromHeight(IPEndPoint remote, uint height, bool onlyHeader = false) {
            var args = new BDict {
                ["cmd"] = "get_block",
                ["height"] = height,
            };
            if (onlyHeader) {
                args["only_header"] = true;
            }
            if (await GetData(remote, args) is not byte[] data) return null;
            var result = new Block(data, hasTx: !onlyHeader);
            if (result.Height != height) return null;
            return result;
        }

        public async Task<Block?> GetBlockFromHash(IPEndPoint remote, Bytes<Hash256> hash, bool onlyHeader = false) {
            var args = new BDict {
                ["cmd"] = "get_block",
                ["hash"] = hash.ToArray(),
            };
            if (onlyHeader) {
                args["only_header"] = true;
            }
            if (await GetData(remote, args) is not byte[] data) return null;
            var result = new Block(data, hasTx: !onlyHeader);
            if (result.Hash != hash) return null;
            return result;
        }

        [CommandHandler("get_blocks")]
        private async ValueTask __GetMultiValue_GetBlocks(NetworkStream stream, BDict args) {
            uint beginHeight = (uint)args["begin_height"];
            if (beginHeight > client.System.LastBlock.Height) goto NotFound;
            if (beginHeight < 1) beginHeight = 1;

            await Bencode.EncodeAsync(stream, new BDict { ["result"] = true }, client.NetworkPrefix, client.CloseCancellationToken);

            bool onlyHeader = args["only_header"] is BBool(true);
            for (uint h = beginHeight; h <= client.System.LastBlock.Height; h++) {
                byte[] blockRawData;
                if (onlyHeader) {
                    var hash = client.System.DB.GetBlockHash(h);
                    Debug.Assert(hash != default);
                    blockRawData = client.System.DB.GetBlockHeader(hash)!;
                } else {
                    Block? block = client.System.DB.GetBlock(h);
                    Debug.Assert(block is not null);
                    blockRawData = block.NetworkSerialize();
                }

                await stream.WriteVarUIntAsync((uint)blockRawData.Length, client.CloseCancellationToken);
                await stream.WriteAsync(blockRawData, client.CloseCancellationToken);
            }

            await stream.WriteVarUIntAsync(0, client.CloseCancellationToken);

            return;
        NotFound:
            await Bencode.EncodeNoPrefixAsync(stream, new BDict { ["result"] = false }, client.CloseCancellationToken);
        }

        public async IAsyncEnumerable<Block> GetBlocks(IPEndPoint remote, uint beginHeight, bool onlyHeader = false, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
            using var stream = await GetNetworkStream(remote, cancellationToken);
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, client.CloseCancellationToken);
            await Bencode.EncodeAsync(stream, new BDict { ["cmd"] = "get_blocks", ["begin_height"] = beginHeight, ["only_header"] = onlyHeader }, client.NetworkPrefix, cancellationTokenSource.Token);

            var r = (BDict)await Bencode.DecodeAsync(stream, client.NetworkPrefix, cancellationToken: cancellationTokenSource.Token);
            if (r["result"] is not BBool(true)) yield break;
            while (await stream.ReadVarUIntAsync(cancellationTokenSource.Token) is (ulong length and not 0 and < int.MaxValue, _)) {
                byte[] blockRawData = await stream.ReadBytesAsync((int)length, cancellationTokenSource.Token);
                yield return new Block(blockRawData, hasTx: !onlyHeader);
            }
        }



        [CommandHandler("get_tx")]
        private bool __GetValue_Transaction(GetValueEventArgs e) {
            var hash = new Bytes<Hash256>((byte[])e.Dict["hash"]);
            if (client.System.DB.GetTransactionRawData(hash) is byte[] txData) {
                e.Value = txData;
                return true;
            }
            return false;
        }

        public async Task<Transaction?> GetTransaction(IPEndPoint remote, Bytes<Hash256> hash) {
            var args = new BDict { ["hash"] = hash.ToArray() };
            if (await GetData(remote, args) is not byte[] data) return null;
            return Transaction.NetworkDeserialize(data);
        }

        #endregion


        public async ValueTask DisposeAsync() {
            ValueTask task = udpServer.DisposeAsync();
            tcpServer.Dispose();
            await task.ConfigureAwait(false);
        }


    }
}

#pragma warning restore IDE1006 // 命名样式