#nullable enable

using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace OnlyChain.Network {
    public sealed class KBucket : IEnumerable<Node>, IReadOnlyDictionary<Address, Node> {
        readonly static Random random = new Random();
        /// <summary>
        /// 表示节点需要重新ping的时间间隔（不能大于63s）
        /// </summary>
        public readonly static TimeSpan Timeout = TimeSpan.FromMinutes(0.5);

        [System.Diagnostics.DebuggerDisplay("{Node}")]
        private struct Slot : IEquatable<Slot>, IDisposable {
            private readonly Timer refreshTask;
            public readonly Node Node;

            public Slot(Node node, Action<Node> timeoutAction) {
                Node = node;
                var dueTime = Timeout - (DateTime.Now - node.RefreshTime) - TimeSpan.FromSeconds(random.NextDouble() * 3); // 加上微量扰动，防止同一时间发起大量ping请求
                if (dueTime < TimeSpan.Zero) dueTime = TimeSpan.Zero;
                // refreshTask = null!;
                refreshTask = new Timer(n => timeoutAction((Node)n!), node, dueTime, Timeout + TimeSpan.FromSeconds(random.NextDouble()));
            }

            public bool Equals(Slot other) => Node.Address == other.Node.Address;

            public void Dispose() => refreshTask.Dispose();
        }

        //[System.Diagnostics.DebuggerDisplay("{Node}")]
        //private sealed class Slot {
        //    public Node? Node { get; set; }

        //    public Slot(Node node) => Node = node;
        //}

        public enum AddResult : byte {
            Success, IsSelf, Existed, Overflow
        }

        public readonly int K;
        public readonly Address MyAddress;
        private readonly SortedDictionary<DateTime, Slot> historyRefreshRecord = new SortedDictionary<DateTime, Slot>();
        //private readonly Timer refreshTimer;
        private readonly SortedList<Address, Slot>[] buckets = new SortedList<Address, Slot>[Address.Size * 8];
        private readonly Func<Node, Task<bool>> ping;

        /// <summary>
        /// 只是参考数量，实际数量会有波动。
        /// </summary>
        public int Count => buckets.Sum(bucket => bucket.Count);

        public IEnumerable<Address> Keys => throw new NotImplementedException();

        public IEnumerable<Node> Values => throw new NotImplementedException();

        public Node this[Address key] => throw new NotImplementedException();

        public KBucket(int k, in Address myAddress, Func<Node, Task<bool>> ping) {
            if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));

            K = k;
            MyAddress = myAddress;
            this.ping = ping ?? throw new ArgumentNullException(nameof(ping));
            for (int i = 0; i < buckets.Length; i++) {
                buckets[i] = new SortedList<Address, Slot>(k + 1, Comparer<Address>.Create((a, b) => (MyAddress ^ a).CompareTo(MyAddress ^ b)));
            }

            //refreshTimer = new Timer(async delegate {
            //    if (!Monitor.TryEnter(historyRefreshRecord)) return;

            //    IReadOnlyList<KeyValuePair<DateTime, Slot>>? slots = null;
            //    try {
            //        var beginTime = DateTime.Now - Timeout;
            //        slots = historyRefreshRecord.TakeWhile(kv => kv.Key <= beginTime).ToList();
            //        foreach (var s in slots) {
            //            historyRefreshRecord.Remove(s.Key);
            //        }
            //    } finally {
            //        Monitor.Exit(historyRefreshRecord);
            //    }

            //    if (slots.Count == 0) return;

            //    async Task Ping(Slot s) {
            //        if (await ping(s.Node!)) {
            //            s.Node!.RefreshTime = DateTime.Now;
            //            AddRefreshRecord(s);
            //        } else {
            //            Console.WriteLine($"remove: {s.Node!.Address}");
            //            Remove(s.Node!.Address);
            //        }
            //    }

            //    var pingTasks = slots.Where(kv => kv.Value.Node != null).Select(s => Ping(s.Value)).ToList();
            //    foreach (var task in pingTasks) await task;
            //}, null, TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5));
        }

        /// <summary>
        /// 添加新节点到K桶。
        /// <para>如果K桶满了：lookup为true时，添加新节点，然后移除距离自身最远的节点。否则不做任何操作。</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="lookup">该值为true表示新节点是通过find_node请求到的</param>
        /// <returns></returns>
        public async ValueTask<AddResult> Add(Node node, bool lookup) {
            int index = (node.Address ^ MyAddress).Log2;
            if (index < 0) return AddResult.IsSelf;

            var bucket = buckets[index];
            Task<AddResult>? pingTask = null;
            lock (bucket) {
                async void TimeoutAction(Node n) {
                    if (await ping(n)) {
                        n.RefreshTime = DateTime.Now;
                    } else {
                        lock (bucket) {
                            if (bucket.Remove(n.Address, out var slot)) slot.Dispose();
                        }
                    }
                }

                if (bucket.TryGetValue(node.Address, out var existedSlot)) {
                    if (existedSlot.Node!.IPEndPoint.Equals(node.IPEndPoint) || existedSlot.Node!.RefreshTime >= node.RefreshTime) return AddResult.Existed;
                    // 如果新节点与旧节点IP端口不一致，并且新节点较新，那么ping一下旧节点，若旧节点无响应，则使用新节点代替旧节点
                    pingTask = ping(existedSlot.Node!).ContinueWith(task => {
                        if (task.Result) return AddResult.Existed;
                        lock (bucket) {
                            if (bucket.Remove(node.Address, out var slot)) {
                                slot.Dispose();
                                bucket.Add(node.Address, new Slot(node, TimeoutAction));
                                return AddResult.Success;
                            }
                        }
                        return AddResult.Overflow;
                    });
                } else {
                    if (bucket.Count < K) {
                        bucket.Add(node.Address, new Slot(node, TimeoutAction));
                    } else if (lookup) {
                        if ((node.Address ^ MyAddress) < (bucket.Values[^1].Node.Address ^ MyAddress)) {
                            bucket.Values[^1].Dispose();
                            bucket.RemoveAt(bucket.Count - 1);
                            bucket.Add(node.Address, new Slot(node, TimeoutAction));
                        }
                    } else {
                        return AddResult.Overflow;
                    }
                }
            }

            return pingTask is { } ? await pingTask : AddResult.Success;
        }

        public bool Remove(Address address) {
            int index = (address ^ MyAddress).Log2;
            if (index < 0) return false;

            var bucket = buckets[index];
            lock (bucket) {
                if (bucket.Remove(address, out var slot)) {
                    slot.Dispose();
                    return true;
                }
            }
            return false;
        }

        public Node[] FindNode(Address target, int count) {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return Array.Empty<Node>();

            Address diff = MyAddress ^ target;
            int diffIndex = diff.Log2;
            var result = new List<Node>(count + K);
            var prevCount = 0;
            for (int i = diffIndex; i >= 0; i--) {
                if (diff.Bit(i)) {
                    var bucket = buckets[i];
                    prevCount = result.Count;
                    lock (bucket) result.AddRange(bucket.Select(s => s.Value.Node!));
                    if (result.Count >= count) goto Return;
                }
            }
            for (int i = 0; i < buckets.Length; i++) {
                if (!diff.Bit(i)) {
                    var bucket = buckets[i];
                    prevCount = result.Count;
                    lock (bucket) result.AddRange(bucket.Select(s => s.Value.Node!));
                    if (result.Count >= count) goto Return;
                }
            }

        Return:
            if (result.Count > count) {
                result.Sort(prevCount, result.Count - prevCount, Comparer<Node>.Create((a, b) => (a.Address ^ target).CompareTo(b.Address ^ target)));
                result.RemoveRange(count, result.Count - count);
            }
            return result.ToArray();
        }

        public Node[] FindNode(Address target, int count, int randomCount) {
            var hashset = new HashSet<Node>(FindNode(target, count));
            var otherNodes = ((IEnumerable<Node>)this).Where(node => !hashset.Contains(node)).ToList();
            if (otherNodes.Count > randomCount) {
                for (int i = 0; i < randomCount; i++) {
                    int j = random.Next(i, otherNodes.Count);
                    (otherNodes[i], otherNodes[j]) = (otherNodes[j], otherNodes[i]);
                }
            }
            return hashset.Concat(otherNodes.Take(randomCount)).ToArray();
        }

        public IEnumerator<Node> GetEnumerator() {
            Node[] nodes;
            foreach (var bucket in buckets) {
                lock (bucket) nodes = bucket.Values.Select(n => n.Node!).ToArray();
                foreach (var n in nodes) {
                    yield return n;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(Address key) {
            int index = (key ^ MyAddress).Log2;
            if (index < 0) return false;

            var bucket = buckets[index];
            lock (bucket) return bucket.ContainsKey(key);
        }

        public bool TryGetValue(Address key, [MaybeNullWhen(false)] out Node value) {
            int index = (key ^ MyAddress).Log2;
            if (index < 0) {
                value = default!;
                return false;
            }

            var bucket = buckets[index];
            lock (bucket) {
                if (bucket.TryGetValue(key, out var slot)) {
                    value = slot.Node;
                    return true;
                }
                value = default!;
                return false;
            }
        }

        IEnumerator<KeyValuePair<Address, Node>> IEnumerable<KeyValuePair<Address, Node>>.GetEnumerator()
            => ((IEnumerable<Node>)this).Select(node => new KeyValuePair<Address, Node>(node.Address, node)).GetEnumerator();
    }
}
