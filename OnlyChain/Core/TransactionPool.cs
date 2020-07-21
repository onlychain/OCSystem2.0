#nullable enable

using OnlyChain.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace OnlyChain.Core {
    public sealed class TransactionPool {
        // 按每个账户平均GasPrice排序

        private readonly object locker = new object();
        private readonly Dictionary<Address, SortedSet<Transaction>> allTransactions = new Dictionary<Address, SortedSet<Transaction>>();
        private readonly SortedSet<Address> sortedTransactions;

        public int Count { get; private set; }

        public TransactionPool() {
            sortedTransactions = new SortedSet<Address>(new AddressComparer(allTransactions));
        }

        public void Push(Transaction tx) {
            lock (locker) {
                if (allTransactions.TryGetValue(tx.From, out SortedSet<Transaction>? list)) {
                    if (list.Contains(tx)) return;
                    sortedTransactions.Remove(tx.From);
                    list.Add(tx);
                } else {
                    list = new SortedSet<Transaction>(TransactionComparer.Instance) {
                        tx
                    };
                    allTransactions.Add(tx.From, list);
                }
                sortedTransactions.Add(tx.From);
                Count++;
            }
        }

        public Transaction? Pop() {
            lock (locker) {
                if (Count is 0) return null;

                Address first = sortedTransactions.Min;
                sortedTransactions.Remove(first);
                SortedSet<Transaction> list = allTransactions[first];
                Transaction result = list.Min!;
                if (list is { Count: 1 }) {
                    allTransactions.Remove(first);
                } else {
                    list.Remove(result);
                    sortedTransactions.Add(first);
                }
                Count--;
                return result;
            }
        }

        public bool Remove(Transaction tx) {
            lock (locker) {
                if (Count is 0) return false;

                if (sortedTransactions.Remove(tx.From) is false) return false;
                SortedSet<Transaction> list = allTransactions[tx.From];
                if (list.Remove(tx) is false) return false;
                if (list is { Count: 0 }) {
                    allTransactions.Remove(tx.From);
                } else {
                    sortedTransactions.Add(tx.From);
                }
                return true;
            }
        }


        sealed class AddressComparer : IComparer<Address> {
            private readonly IReadOnlyDictionary<Address, SortedSet<Transaction>> map;

            public AddressComparer(IReadOnlyDictionary<Address, SortedSet<Transaction>> map) => this.map = map;

            public int Compare(Address x, Address y) {
                if (x == y) return 0;
                double xVal = 0, yVal = 0;
                if (map.TryGetValue(x, out var xList)) {
                    xVal = xList.Average(tx => (long)tx.GasPrice);
                }
                if (map.TryGetValue(y, out var yList)) {
                    yVal = yList.Average(tx => (long)tx.GasPrice);
                }
                return yVal.CompareTo(xVal);
            }
        }

        sealed class TransactionComparer : IComparer<Transaction> {
            public static readonly TransactionComparer Instance = new TransactionComparer();

            private TransactionComparer() { }

            public int Compare([AllowNull] Transaction x, [AllowNull] Transaction y) {
                return x!.Nonce.CompareTo(y!.Nonce);
            }
        }
    }
}
