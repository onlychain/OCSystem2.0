using OnlyChain.Core;
using OnlyChain.Coding;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using OnlyChain.Network.Objects;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using OnlyChain.Network;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using OnlyChain.Database;
using System.Collections;
using System.Diagnostics;

namespace OnlyChain {
    class Program {
        static string[] messages;

        static byte[] TestArray => new byte[0];

        sealed class StringHashAlgorithm : MerklePatriciaTree<Address, string, Hash<Size256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Address, string, Hash<Size256>>.IHashAlgorithm Default = new StringHashAlgorithm();

            private readonly static SHA256 sha256 = SHA256.Create();

            private StringHashAlgorithm() { }

            public Hash<Size256> ComputeHash(Address key, in string value) {
                Hash<Size256> result = default;
                sha256.TryComputeHash(Encoding.UTF8.GetBytes(value), result.Span, out _);
                return result;
            }

            public Hash<Size256> ComputeHash(ReadOnlySpan<Hash<Size256>> hashes) {
                Hash<Size256> result = default;
                sha256.TryComputeHash(MemoryMarshal.Cast<Hash<Size256>, byte>(hashes), result.Span, out _);
                return result;
            }
        }

        unsafe static void Test() {
            Size256 a = default, b = default;

            var sw = new System.Diagnostics.Stopwatch();
            //sw.Restart();
            //for (int i = 0; i < 10000000; i++) {
            //    a.Equals(b);
            //}
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            //Console.WriteLine(a.Equals(b));

            sw.Restart();
            for (int i = 0; i < 1000000000; i++) {
                new ReadOnlySpan<byte>(&a, sizeof(Size256)).SequenceEqual(new ReadOnlySpan<byte>(&b, sizeof(Size256)));
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(new ReadOnlySpan<byte>(&a, sizeof(Size256)).SequenceEqual(new ReadOnlySpan<byte>(&b, sizeof(Size256))));
        }

        unsafe static void TestEC() {
            const int Rows = 1024 * 1024;
            const int DataStride = 15;
            const int ErrorStride = 7;

            var random = new Random();
            var data = new byte[DataStride * Rows];
            var ec = new byte[ErrorStride * Rows];
            random.NextBytes(data);

            ErasureCoding.Encode(data, ec, DataStride, ErrorStride);
            var data2 = data.Clone() as byte[];
            var ec2 = ec.Clone() as byte[];
            ErasureCodingIndex[] indexes = {
                (0, 0), (3, 1), (5, 3), (6, 5), (7, 4), (9, 2)
            };
            for (int i = 0; i < Rows; i++) {
                foreach (var (dataMissIndex, errorIndex) in indexes) {
                    data2[i * DataStride + dataMissIndex] = ec2[i * ErrorStride + errorIndex];
                }
            }
            var dataClone = data.Clone() as byte[];
            ErasureCoding.Decode(data2, DataStride, indexes);
            if (!data.AsSpan().SequenceEqual(data2)) throw new Exception();

            var sw = new System.Diagnostics.Stopwatch();

            sw.Restart();
            for (int i = 0; i < 50; i++) {
                ErasureCoding.Encode(data, ec, DataStride, ErrorStride);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            GC.Collect();

            sw.Restart();
            for (int i = 0; i < 50; i++) {
                ErasureCoding.Decode(dataClone.Clone() as byte[], DataStride, indexes);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        unsafe static void TestRipemd160() {
            var sw = new System.Diagnostics.Stopwatch();
            const int N = 1000000;
            var hash = Hash<Size256>.Empty;

            Hash<Size160> result = new Hash<Size160>();

            sw.Restart();
            for (int i = 0; i < N; i++) {
                result = Ripemd160.ComputeHash(hash);
            }
            sw.Stop();
            Console.WriteLine(new TimeSpan(sw.Elapsed.Ticks / N));
            Console.WriteLine(result.ToString());
        }

        static void TestMPT() {
            var keys = new Address[100];
            for (int i = 0; i < keys.Length; i++) keys[i] = Address.Random();

            var tree = new MerklePatriciaTree<Address, string, Hash<Size256>>(0);
            for (int i = 0; i < keys.Length; i++) {
                tree.Add(keys[i], keys[i].ToString());
            }
            tree.ComputeHash(StringHashAlgorithm.Default);

            var b1 = tree.FindHashTree(keys[0], out var hashTree);
            var b2 = tree.FindHashTree(keys[1], out var hashTree2);
            var b3 = tree.FindHashTree(keys[2], out var hashTree3);
            var hashTree4 = hashTree.Combine(hashTree2).Combine(hashTree3);
        }

        static void TestHashes() {
            var hashes = new Hashes<Address>("hash160.hashes");
            var hashtable = new Hashtable();
            var keys = new Address[1000_0000];
            for (int i = 0; i < keys.Length; i++) keys[i] = Address.Random();

            var tasks = new Task[16];
            var indexes = new int[keys.Length];
            int startIndex = hashes.Count;

            {
                var sw = new System.Diagnostics.Stopwatch();

                sw.Restart();
                for (int i = 0; i < indexes.Length / 2; i++) {
                    hashtable.Add(keys[i], null);
                }
                sw.Stop();
                Console.WriteLine($"hashtable: {sw.Elapsed}");

                sw.Restart();
                for (int i = 0; i < indexes.Length / 2; i++) {
                    indexes[i] = hashes.Add(keys[i]);
                }
                sw.Stop();
                Console.WriteLine($"hashes: {sw.Elapsed}");

                //    sw.Restart();
                //    for (int i = indexes.Length / 2; i < indexes.Length; i++) {
                //        if (hashes.GetIndex(keys[i]) >= 0) throw new Exception();
                //    }
                //    sw.Stop();
                //    Console.WriteLine($"{sw.Elapsed}");
                //    return;
            }

            for (int n = 0; n < tasks.Length; n++) {
                int taskId = n;
                tasks[n] = Task.Run(() => {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Restart();
                    for (int i = 0; i < indexes.Length / 2; i++) {
                        indexes[i] = hashes.GetIndex(keys[i]);
                        if (indexes[i] != i + startIndex) throw new Exception();
                    }
                    sw.Stop();
                    Console.WriteLine($"{taskId}, {sw.Elapsed}");
                });
            }
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Restart();
                for (int i = indexes.Length / 2; i < indexes.Length; i++) {
                    indexes[i] = hashes.Add(keys[i]);
                    // if (hashes.GetIndex(keys[i]) >= 0) throw new Exception();
                }
                sw.Stop();
                Console.WriteLine($"hashes: {sw.Elapsed}");
                Console.WriteLine(hashes.Count);
            }
            Task.WaitAll(tasks);

            Console.WriteLine("=================");

            for (int n = 0; n < tasks.Length; n++) {
                int taskId = n;
                tasks[n] = Task.Run(() => {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Restart();
                    for (int i = 0; i < indexes.Length / 2; i++) {
                        indexes[i] = hashes.GetIndex(keys[i]);
                        if (indexes[i] != i + startIndex) throw new Exception();
                    }
                    sw.Stop();
                    Console.WriteLine($"{taskId}, {sw.Elapsed}");
                });
            }

            Task.WaitAll(tasks);
        }

        static void TestSecp256k1() {
            var msg = new byte[32];
            var random = new Random();
            random.NextBytes(msg);
            var privkey = Secp256k1.Secp256k1.CreatePrivateKey();
            //while (true) {
            //    var sign = Secp256k1.Secp256k1.Sign(privkey, msg);
            //    var pubkey = Secp256k1.Secp256k1.CreatePublicKey(privkey);
            //    bool result = Secp256k1.Secp256k1.Verify(pubkey, msg, sign);
            //    if (result is false) {
            //        Console.WriteLine("!!!!!!!");
            //        break;
            //    }
            //    Console.Write('.');
            //}
            const int N = 10000;

            var sw = new Stopwatch();
            sw.Restart();
            for (int i = 0; i < N; i++) {
                Secp256k1.Secp256k1.CreatePublicKey(privkey);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            sw.Restart();
            for (int i = 0; i < N; i++) {
                Secp256k1.Secp256k1.Sign(privkey, msg);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            var pubkey = Secp256k1.Secp256k1.CreatePublicKey(privkey);
            var sign = Secp256k1.Secp256k1.Sign(privkey, msg);
            sw.Restart();
            for (int i = 0; i < N; i++) {
                Secp256k1.Secp256k1.Verify(pubkey, msg, sign);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        public static readonly HashSet<Address> AllAddresses = new HashSet<Address>();

        unsafe static void Main(string[] args) {
            TestMPT();
            return;
            //TestHashes();
            //GC.Collect();
            //hashes.Save();
            //Console.WriteLine(hashes.Count);
            //var buffer = Marshal.AllocHGlobal((IntPtr)(1L << 32));
            //for (long i = 0; i < (1L << 32) / 8; i++) ((long*)buffer)[i] = 0;
            //Thread.Sleep(-1);
            //var keys = new Address[10000];
            //for (int i = 0; i < keys.Length; i++) keys[i] = Address.Random();

            ////foreach (var k in keys) Console.WriteLine(k);
            //Console.WriteLine("=====================");

            ////var memSize = GC.GetTotalMemory(true);

            //// 019251639cb90c9d59f2108524d377315d781caf
            //// 8750f447c8355658a4724a129592b5f2c21b87dc
            //// 0f77cce7599bf8bb0b49f00c378410d861bad2af
            ////keys[0] = "019251639cb90c9d59f2108524d377315d781caf";
            ////keys[1] = "8750f447c8355658a4724a129592b5f2c21b87dc";
            ////keys[2] = "0f77cce7599bf8bb0b49f00c378410d861bad2af";

            //var tree = new MerklePatriciaTree<Address, string, Hash<Size256>>(0);
            //sw.Restart();
            //for (int i = 0; i < keys.Length; i++) {
            //    tree.Add(keys[i], keys[i].ToString());
            //}
            //tree.Add("841d447243983f9a26f7b59d2d330b0916e61a99", "841d447243983f9a26f7b59d2d330b0916e61a99");
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            //tree.ComputeHash(StringHashAlgorithm.Default);

            //foreach (var kv in tree) {
            //    //Console.WriteLine(kv.Key);
            //}

            //var b1 = tree.FindHashTree(keys[0], out var hashTree);
            //var b2 = tree.FindHashTree("8000f447c8355658a4724a129592b5f2c21b87dc", out var hashTree2);
            //Console.WriteLine(b1);
            //Console.WriteLine(b2);

            //int count = 0;
            //foreach (var kv in tree) {
            //    count++;
            //}
            //Console.WriteLine(count);

            //count = 0;
            //foreach (var kv in tree) {
            //    count++;
            //}
            //Console.WriteLine(count);

            ////Console.WriteLine((GC.GetTotalMemory(true) - memSize) / 1024.0 / 1024.0);

            ////sw.Restart();
            ////for (int i = 0; i < keys.Length; i++) {
            ////    tree.TryAdd(keys[i], keys[i].ToString());
            ////}
            ////sw.Stop();
            ////Console.WriteLine(sw.Elapsed);
            ////Console.WriteLine(tree.Count);


            //sw.Restart();
            //for (int i = 0; i < keys.Length; i++) {
            //    if (!tree.TryGetValue(keys[i], out _)) throw new Exception();
            //}
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            ////sw.Restart();
            ////foreach (var kv in tree) Console.WriteLine(kv);
            //////foreach (var kv in tree) ;
            ////sw.Stop();
            ////Console.WriteLine(sw.Elapsed);

            //// 1afbacf8ac3e1356c2a5887ded44934daf7fc014ae75deb7ae4fcc4b5410a92f

            ////sw.Restart();
            ////for (int i = 0; i < keys.Length; i++) {
            ////    if (!tree.Remove(keys[i])) throw new Exception();
            ////}
            ////sw.Stop();
            ////Console.WriteLine(sw.Elapsed);

            //count = 0;
            //foreach (var kv in tree) {
            //    count++;
            //    //Console.WriteLine(kv);
            //}
            //Console.WriteLine(count);

            //tree.ComputeHash(StringHashAlgorithm.Default);

            //var mpt2 = tree.NewNext();
            ////for (int i = 0; i < keys.Length; i++) keys[i] = Address.Random();

            //for (int i = 0; i < keys.Length; i++) {
            //    mpt2[keys[i]] = "2";
            //    //mpt2.Add(keys[i], keys[i].ToString());
            //}

            //mpt2.ComputeHash(StringHashAlgorithm.Default);

            //var mpt3 = mpt2.NewNext();
            ////for (int i = 0; i < keys.Length; i++) keys[i] = Address.Random();

            //for (int i = 0; i < keys.Length; i++) {
            //    mpt3[keys[i]] = i.ToString();
            //    //mpt3.Add(keys[i], keys[i].ToString());
            //}

            //mpt3.ComputeHash(StringHashAlgorithm.Default);

            //count = 0;
            //foreach (var kv in tree) {
            //    count++;
            //    //Console.WriteLine(kv);
            //}
            //Console.WriteLine(count);

            //count = 0;
            //foreach (var kv in mpt2) {
            //    count++;
            //    //Console.WriteLine(kv);
            //}
            //Console.WriteLine(count);

            //count = 0;
            //foreach (var kv in mpt3) {
            //    count++;
            //    //Console.WriteLine(kv);
            //}
            //Console.WriteLine(count);

            //Console.WriteLine(tree.RootHash);

            //var tree2 = new MerklePatriciaTree<Address, string, Hash<Size256>>(0);

            //foreach (var (k, v) in tree) {
            //    tree2[k] = v;
            //}

            //tree2.ComputeHash(StringHashAlgorithm.Default);
            //Console.WriteLine(tree2.RootHash);

            //Thread.Sleep(-1);
            //Console.WriteLine(tree.Count);
            //Console.WriteLine(mpt2.Count);
            //Console.WriteLine(mpt3.Count);
            //return;



            //var sw = new System.Diagnostics.Stopwatch();
            //for (int i = 0; i < 10000; i++) {
            //    var h = Ripemd160.ComputeHash(new Hash<Size256>());
            //}
            //var hash256 = new Hash<Size256>("0000000000000000000000000000000000000000000000000000000000000001");
            //sw.Restart();
            //for (int i = 0; i < 200_0000; i++) {
            //    Ripemd160.ComputeHash(hash256);
            //}
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            //Console.WriteLine(Ripemd160.ComputeHash("0000000000000000000000000000000000000000000000000000000000000001"));
            //return;


            /*
            var clients = new List<Client>();
            var bindIP = IPAddress.Parse("127.0.0.1");
            Address address = Address.Random();
            AllAddresses.Add(address);
            if (!AllAddresses.Contains(address)) throw null;
            clients.Add(new Client(address, 0, bindIP: bindIP));
            clients[^1].ReceiveBroadcast += Program_ReceiveBroadcast(clients.Count);
            Console.WriteLine($"{clients.Count,5} create: {address}");

            await Task.Delay(200);
            address = "dbaaf68ee499766bdc548e324cdd204e3a563f2c";
            AllAddresses.Add(address);
            if (!AllAddresses.Contains(address)) throw null;
            clients.Add(new Client(address, 0, bindIP: bindIP, seeds: new[] { new IPEndPoint(bindIP, clients[0].Port) }));
            clients[^1].ReceiveBroadcast += Program_ReceiveBroadcast(clients.Count);
            Console.WriteLine($"{clients.Count,5} create: {address}");

            for (int i = 0; i < 10; i++) {
                await Task.Delay(10);
                address = Address.Random();
                AllAddresses.Add(address);
                if (!AllAddresses.Contains(address)) throw null;
                clients.Add(new Client(address, 0, bindIP: bindIP, seeds: new[] { new IPEndPoint(bindIP, clients[0].Port) }));
                clients[^1].ReceiveBroadcast += Program_ReceiveBroadcast(clients.Count);
                Console.WriteLine($"{clients.Count,5} create: {address}");
            }
            */



            //Address cmpAddress = clients[0].Address;
            //clients.Sort(Comparer<Client>.Create((a, b) => (b.Address ^ cmpAddress).CompareTo(a.Address ^ cmpAddress)));
            //foreach (var c in clients) Console.WriteLine(c.Address);

            //foreach (var c in clients.Skip(5)) {
            //    await c.DisposeAsync();
            //}
            //clients.RemoveRange(5, clients.Count - 5);

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Restart();
            //await clients[0].DisposeAsync();
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);

            //await Task.Delay(2000);

            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Restart();
            //var node = await clients[500].Lookup("dbaaf68ee499766bdc548e324cdd204e3a563f2c");
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            //Console.WriteLine(node);
            //return;

            /*
            int broadcastCount = 0;
            while (true) {
                await Task.Delay(1000);
                if (messages != null) {
                    Console.WriteLine($"======================================= {broadcastCount}");
                    for (int i = 1; i < messages.Length; i++) {
                        if (messages[i] is null) {
                            Console.WriteLine($"{i,4} miss: {clients[i].Address}");
                        }
                    }
                }
                messages = new string[clients.Count];
                clients[0].Broadcast(Encoding.UTF8.GetBytes("hello"));
                broadcastCount++;
            }

            await Task.Delay(10);
            Console.WriteLine("=======================================");
            foreach (var n in clients[^1].Nodes) {
                Console.WriteLine(n);
            }
            Console.WriteLine("=======================================");
            Console.WriteLine(clients[^1].Address);
            var random = new Random();
            var randomAddress = new byte[Address.Size];
            random.NextBytes(randomAddress);
            for (int i = 0; i < 16; i++) {
                Console.WriteLine($"=======================================");
                randomAddress[0] = (byte)(i << 4);
                Address target = new Address(randomAddress);
                Console.WriteLine($"find {target}");
                foreach (var n in clients[^1].Nodes.FindNode(target, 10)) {
                    Console.WriteLine(n);
                }
            }

            await Task.Delay(-1);

            Console.WriteLine(clients.Count);
            */
        }

        private static EventHandler<BroadcastEventArgs> Program_ReceiveBroadcast(int index) {
            return (object sender, BroadcastEventArgs e) => {
                Client client = (Client)sender;
                string message = Encoding.UTF8.GetString(e.Message);
                //Console.WriteLine($"{index,5}, {client.Address}: {message}");
                messages[index - 1] = message;
            };
        }
    }
}
