#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using OnlyChain.Network.Objects;
using System.Threading.Channels;
using System.IO;
using System.Reflection;
using System.Linq;
using OnlyChain.Core;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;

namespace OnlyChain.Network {
    public sealed class UdpServer : IAsyncDisposable {
        static int requestCancelCount = 0;
        //static readonly Timer requestCancelCounterTimer = new Timer(delegate {
        //    var count = Interlocked.Exchange(ref requestCancelCount, 0);
        //    Console.WriteLine("cancel: " + count);
        //}, null, TimeSpan.FromSeconds(3.5), TimeSpan.FromSeconds(3.5));

        /// <summary>
        /// 用于管理本地请求的数据结构
        /// </summary>
        private sealed class RequestList {
            static readonly Random random = new Random();

            private readonly List<LocalRequest?> requests = new List<LocalRequest?>(128);
            private ulong minIndex, autoIndex;

            unsafe public RequestList() {
                ulong t;
                lock (random) random.NextBytes(new Span<byte>(&t, sizeof(ulong)));
                autoIndex = t >> 1;
                minIndex = autoIndex;
            }

            public LocalRequest NewRequest(BDict dict, CancellationToken cancellationToken = default) {
                LocalRequest result;
                lock (requests) {
                    result = new LocalRequest(autoIndex++, dict, requestTimeout, cancellationToken);
                    requests.Add(result);
                }
                result.CancellationTokenSource.Token.Register(() => {
                    Pop(result.Index);
                    result.Response.TrySetCanceled(result.CancellationTokenSource.Token);
                });
                return result;
            }

            /// <summary>
            /// 根据请求编号弹出对应的请求
            /// </summary>
            /// <param name="requestIndex"></param>
            /// <returns></returns>
            public LocalRequest? Pop(ulong requestIndex) {
                lock (requests) {
                    if (requestIndex >= minIndex && requestIndex - minIndex < (ulong)requests.Count) {
                        if (requests[(int)(requestIndex - minIndex)] is LocalRequest r) {
                            requests[(int)(requestIndex - minIndex)] = null;

                            int i = 0;
                            while (i < requests.Count && requests[i] == null) i++;
                            if (i > 0) {
                                requests.RemoveRange(0, i);
                                minIndex += (ulong)i;
                            }
                            return r;
                        }
                    }
                }
                return null;
            }

            public void RemoveAll(params ulong[] requestIndexes) {
                lock (requests) {
                    foreach (var requestIndex in requestIndexes) {
                        if (requestIndex >= minIndex && requestIndex - minIndex < (ulong)requests.Count) {
                            requests[(int)(requestIndex - minIndex)] = null;
                        }
                    }
                    int i = 0;
                    while (i < requests.Count && requests[i] == null) i++;
                    requests.RemoveRange(0, i);
                    minIndex += (ulong)i;
                }
            }

            public void TryRemoveTimeout(TimeSpan timeout) {
                if (!Monitor.TryEnter(requests)) {
                    if (requests.Count < 1000) return; // requests.Count 不需要加锁，因为不需要那么精准的数值
                    Monitor.Enter(requests);
                }
                try {
                    var filterTime = DateTime.UtcNow - timeout;
                    int i = 0;
                    for (; i < requests.Count; i++) {
                        if (requests[i] is LocalRequest r && r.DateTime > filterTime) {
                            break;
                        }
                    }
                    requests.RemoveRange(0, i);
                    minIndex += (ulong)i;
                } finally {
                    Monitor.Exit(requests);
                }
            }

            public LocalRequest? this[ulong requestIndex] {
                get {
                    lock (requests) {
                        if (requestIndex < minIndex && requestIndex >= minIndex + (ulong)requests.Count) return null;
                        return requests[(int)(requestIndex - minIndex)];
                    }
                }
            }
        }

        static readonly TimeSpan requestTimeout = TimeSpan.FromSeconds(2);

        private readonly IClient client;
        private readonly Socket udpSocket;
        private readonly Task[] serverTasks = new Task[4];
        private readonly Dictionary<string, Func<RemoteRequest, Task>> requestHandlers = new Dictionary<string, Func<RemoteRequest, Task>>();
        private readonly RequestList requestList = new RequestList();

        private int processCount = 0;
        private readonly Timer processCountTask;

        public IPEndPoint IPEndPoint => (IPEndPoint)udpSocket.LocalEndPoint!;

        public int TPS { get; private set; }

        public UdpServer(IClient client) {
            this.client = client;

            #region Socket相关
            udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            //udpSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            udpSocket.Bind(client.EndPoint);
            const int IOC_IN = unchecked((int)0x80000000);
            const int IOC_VENDOR = 0x18000000;
            const int SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            udpSocket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
            #endregion

            processCountTask = new Timer(delegate {
                TPS = processCount;
                //if (processCount > 0) Console.WriteLine(processCount);
                Interlocked.Exchange(ref processCount, 0);
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void Start() {
            #region 通过反射将远程请求Handler绑定到Client方法上
            Type type = client.P2P.GetType();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (method.GetCustomAttribute<CommandHandlerAttribute>() is not CommandHandlerAttribute attr) continue;
                var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                if (!(paramTypes is { Length: 1 }) || paramTypes[0] != typeof(RemoteRequest)) continue;

                if (method.ReturnType == typeof(BDict)) {
                    var handler = (Func<RemoteRequest, BDict?>)method.CreateDelegate(typeof(Func<RemoteRequest, BDict?>), client.P2P);
                    requestHandlers.Add(attr.Command, dict => Task.FromResult(handler(dict)));
                } else if (method.ReturnType == typeof(ValueTask<BDict>)) {
                    var handler = (Func<RemoteRequest, ValueTask<BDict?>>)method.CreateDelegate(typeof(Func<RemoteRequest, ValueTask<BDict?>>), client.P2P);
                    requestHandlers.Add(attr.Command, dict => handler(dict).AsTask());
                } else if (method.ReturnType == typeof(Task<BDict>)) {
                    var handler = (Func<RemoteRequest, Task<BDict?>>)method.CreateDelegate(typeof(Func<RemoteRequest, Task<BDict?>>), client.P2P);
                    requestHandlers.Add(attr.Command, dict => handler(dict));
                } else if (method.ReturnType == typeof(void)) {
                    var handler = (Action<RemoteRequest>)method.CreateDelegate(typeof(Action<RemoteRequest>), client.P2P);
                    requestHandlers.Add(attr.Command, dict => {
                        handler(dict);
                        return Task.CompletedTask;
                    });
                } else if (method.ReturnType == typeof(ValueTask)) {
                    var handler = (Func<RemoteRequest, ValueTask>)method.CreateDelegate(typeof(Func<RemoteRequest, ValueTask>), client.P2P);
                    requestHandlers.Add(attr.Command, async dict => await handler(dict));
                } else if (method.ReturnType == typeof(Task)) {
                    var handler = (Func<RemoteRequest, Task>)method.CreateDelegate(typeof(Func<RemoteRequest, Task>), client.P2P);
                    requestHandlers.Add(attr.Command, handler);
                }
            }
            #endregion

            for (int i = 0; i < serverTasks.Length; i++) {
                serverTasks[i] = StartReceive();
            }
        }

        private async Task StartReceive() {
            EndPoint remoteEP = new IPEndPoint(IPAddress.IPv6None, 0);
            var buffer = new byte[4096];
            while (!client.CloseCancellationToken.IsCancellationRequested) {
                try {
                    var result = await udpSocket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEP);
                    var obj = Bencode.Decode(new MemoryStream(buffer, 0, result.ReceivedBytes), client.NetworkPrefix, maxReadSize: 2048);

                    if (obj is not BDict dict) continue; // 不是字典类型

                    if (dict["i"] is not BUInt(ulong packageIndex)) continue; // 不存在i(包序号)字段
                    if (dict["a"] is not BAddress(Bytes<Address> senderAddress)) continue; // 不存在a(发送者id)字段
                    if (dict["c"] is BString(string cmd)) { // 存在c(命令)字段，表示远端请求
                        if (!requestHandlers.TryGetValue(cmd, out var handler)) continue;

                        var responseTask = handler(new RemoteRequest(dict, (IPEndPoint)result.RemoteEndPoint, senderAddress));
                        if (responseTask.IsCompleted) {
                            if (responseTask is Task<BDict?> { Result: BDict response }) {
                                response["i"] = packageIndex;
                                Send(response, result.RemoteEndPoint);
                            }
                        } else if (responseTask is Task<BDict?> hasResultTask) {
                            _ = hasResultTask.ContinueWith(task => {
                                try {
                                    if (client.CloseCancellationToken.IsCancellationRequested) return;
                                    var response = task.Result;
                                    if (response is null) return;
                                    response["i"] = packageIndex;
                                    Send(response, result.RemoteEndPoint);
                                } catch { }
                            });
                        }

                        Interlocked.Increment(ref processCount);
                    } else { // 表示远端响应
                        if (requestList.Pop(packageIndex) is LocalRequest request) {
                            request.Response.TrySetResult(new RemoteResponse(packageIndex, senderAddress, (IPEndPoint)result.RemoteEndPoint, dict));
                        }
                    }
                } catch (TaskCanceledException) {
                } catch {
                }
            }
        }

        public void Send(BDict dict, EndPoint remote) {
            dict["a"] = client.Address;
            var message = Bencode.Encode(dict, client.NetworkPrefix);
            Send(message, remote);
        }

        internal async void Send(byte[] message, EndPoint remote) {
            await udpSocket.SendToAsync(message, SocketFlags.None, remote);
        }

        public async Task<RemoteResponse> Request(BDict dict, EndPoint remote, int retryCount = 2, CancellationToken cancellationToken = default) {
            if (retryCount < 1) throw new ArgumentOutOfRangeException(nameof(retryCount), "重试次数不能小于1");

            int count = 1;

            if (cancellationToken.CanBeCanceled) {
                var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cancellationToken = cancellationTokenSource.Token;
                cancellationToken.Register(delegate { count = retryCount; });
            }

        Retry:
            try {
                var request = requestList.NewRequest(dict, cancellationToken);
                dict["i"] = request.Index;
                Send(dict, remote);
                return await request.Response.Task;
            } catch (TaskCanceledException) when (count < retryCount) {
                count++;
                Interlocked.Increment(ref requestCancelCount);
                goto Retry;
            }
        }

        #region IDisposable Support
        private bool isDisposed = false;

        async ValueTask Dispose(bool disposing) {
            if (!isDisposed) {
                try {
                    udpSocket.Shutdown(SocketShutdown.Both);
                } catch { }
                try {
                    udpSocket.Close();
                } catch { }
                foreach (var task in serverTasks) await task;

                if (disposing) {
                    await processCountTask.DisposeAsync();
                    udpSocket.Dispose();
                    foreach (var task in serverTasks) task.Dispose();
                    GC.SuppressFinalize(this);
                }
                isDisposed = true;
            }
        }

        ~UdpServer() {
            Dispose(false).AsTask().Wait();
        }

        public ValueTask DisposeAsync() => Dispose(true);
        #endregion
    }
}
