#nullable enable

using Microsoft.VisualBasic;
using OnlyChain.Model;
using OnlyChain.Network;
using OnlyChain.Network.Objects;
using OnlyChain.Secp256k1;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class ProducerSystem : IDisposable {
        static readonly TimeSpan TimeSpanRange = TimeSpan.FromMinutes(3); // 区块时间戳超过当前系统时间3分钟之外认为无效
        const int NextBlockMilliseconds = 1000; // 允许跨见证人的间隔时间(ms)

        private readonly IClient client;
        private readonly TransactionPool transactionPool = new(); // 交易池
        private readonly Blacklist<Bytes<Hash256>> blacklistTransactions = new(Constants.BlacklistTxCount); // 交易黑名单
        private readonly LimitedTimeDictionary<Bytes<Hash160>, BlockChipCollection> blockChipCollectionDict = new(TimeSpan.FromSeconds(5));
        private readonly ConcurrentBag<(SuperNode Node, CommitVote Vote)> precommitVotes = new();
        private readonly ConcurrentBag<(SuperNode Node, CommitVote Vote)> commitVotes = new();
        private Stopwatch? lastWaitTimer;
        private CancellationTokenSource? cancellationTokenSource;
        private BlockState? nextRoundBlockState = null;
        private Stopwatch? myBlockTimer; // 本节点出块冷却计时器


        private DateTime lastDateTime = DateTime.UtcNow; // 上一次收到区块的本地时间

        private byte[] PrivateKey => client.SuperConfig!.PrivateKey;
        private BlockChainSystem System => client.System;
        private Bytes<Address>[] SortedCampaignNodes => System.LastBlock.CommitState!.SortedCampaignNodes;

        public ProducerSystem(IClient client) {
            this.client = client;

            client.System.StateTransferred += System_StateTransferred;
        }

        private async void P2P_SuperConnected(object? sender, SuperConnectEventArgs e) {
            if (!System.InProduction) return;

            if (!System.CampaignNodes.TryGetValue(e.SuperNode.Address, out var peer) || peer is null or { IsOnline: false }) {
                peer = new SuperPeer(e.SuperNode);
                Console.WriteLine($"收到超级节点连接：{e.SuperNode.Address}[{e.SuperNode.IPEndPoint}]");
                if (!await client.P2P.SuperConnect(peer)) Console.WriteLine("连接失败");
                System.CampaignNodes[e.SuperNode.Address] = peer;
            }
        }

        /// <summary>
        /// 根据当前出块人获取之后所有出块人的列表
        /// </summary>
        /// <param name="currentProducer"></param>
        /// <returns></returns>
        private IReadOnlyList<int>? GetNextProducers(Bytes<Address> currentProducer) {
            Debug.Assert(System.LastBlock.CommitState is not null);

            BlockState state = System.LastBlock.CommitState;
            int count = Math.Min(Constants.MinProducerCount, state.SortedCampaignNodes.Length);
            int index = state.SortedCampaignNodes.AsSpan(0, count).IndexOf(currentProducer);
            if (index < 0) return null;
            var result = new int[count];
            for (int i = 0; i < count; i++) {
                result[i] = (index + i + 1) % count;
            }
            return result;
        }

        private IPEndPoint?[] GetNextProducerEP() {
            Debug.Assert(System.LastBlock.CommitState is not null);

            BlockState state = System.LastBlock.CommitState;
            int count = Math.Min(Constants.MinProducerCount, state.SortedCampaignNodes.Length);
            var result = new IPEndPoint?[count];
            if (System.LastBlock.Height == 1) {
                for (int i = 0; i < count; i++) {
                    if (state.SortedCampaignNodes[i] == client.Address) continue;
                    if (System.CampaignNodes.TryGetValue(state.SortedCampaignNodes[i], out var peer)) {
                        result[i] = peer?.IPEndPoint;
                    }
                }
            } else {
                int index = state.SortedCampaignNodes.AsSpan(0, count).IndexOf(System.LastBlock.ProducerAddress);
                Debug.Assert(index >= 0);

                for (int i = 0; i < count; i++) {
                    var addr = state.SortedCampaignNodes[(index + i) % count];
                    if (addr == client.Address) continue;
                    if (System.CampaignNodes.TryGetValue(addr, out var peer)) {
                        result[i] = peer?.IPEndPoint;
                    }
                }
            }
            Array.Reverse(result);
            return result;
        }


        internal void ProducerBlockChipArrived(SuperEventArgs e, ReadOnlyMemory<byte> data) {
            BlockChipArrived(BlockChip.Parse(data.Span), e.Node.PublicKey, e.Node.Address);

            SuperBroadcast("other_chip", data, e.Node.Address);
        }

        internal void OtherBlockChipArrived(SuperEventArgs e, ReadOnlyMemory<byte> data) {
            BlockChipArrived(BlockChip.Parse(data.Span), e.Node.PublicKey, e.Node.Address);
        }

        internal void PrecommitVoteArrived(SuperEventArgs e, ReadOnlyMemory<byte> data) {
            var vote = new CommitVote(data.Span);
            if (vote.IsPrecommit is false) return;
            if (Ecdsa.Verify(e.Node.PublicKey, vote.Hash, vote.Signature) is false) return;
            PrecommitVoteArrived(e.Node, vote);
        }

        internal void CommitVoteArrived(SuperEventArgs e, ReadOnlyMemory<byte> data) {
            var vote = new CommitVote(data.Span);
            if (vote.IsPrecommit) return;
            if (Ecdsa.Verify(e.Node.PublicKey, vote.Hash, vote.Signature) is false) return;
            CommitVoteArrived(e.Node, vote);
        }


        private void BlockChipArrived(BlockChip blockChip, PublicKey producerPublicKey, Bytes<Address> producerAddress) {
            if (blockChipCollectionDict.TryGetValue(blockChip.Id, out var blockChipCollection)) {
                blockChipCollection.Add(blockChip);
                return;
            }

            if (lastWaitTimer is not null) {
                bool allowProduce = false;
                int beginIndex = 0;
                if (System.LastBlock.SmallRound.IsValid) {
                    beginIndex = System.LastBlock.SmallRound.IndexInRound + 1;
                }
                int indexRange = Math.Clamp((int)Math.Floor((double)lastWaitTimer.ElapsedMilliseconds / NextBlockMilliseconds), 0, Constants.MinProducerCount);
                for (int i = 0; i < indexRange; i++) {
                    if (SortedCampaignNodes[(beginIndex + i) % Constants.MinProducerCount] == producerAddress) {
                        allowProduce = true;
                        break;
                    }
                }
                if (allowProduce is false) return;
            }

            blockChipCollectionDict.Add(blockChip.Id, new BlockChipCollection(blockChip, producerAddress, producerPublicKey));
        }

        private void PrecommitVoteArrived(SuperNode node, CommitVote precommitVote) {
            precommitVotes.Add((node, precommitVote));
        }

        private void CommitVoteArrived(SuperNode node, CommitVote commitVote) {
            commitVotes.Add((node, commitVote));
        }

        private async void SuperBroadcast(string cmd, ReadOnlyMemory<byte> data, Bytes<Address> beginAddress) {
            IReadOnlyList<int>? forwardIndexes = GetNextProducers(beginAddress);
            if (forwardIndexes is null) return;

            BDict dict = new BDict { ["cmd"] = cmd, ["length"] = data.Length };

            var tasks = new List<Task>();
            for (int i = 0; i < forwardIndexes.Count - 1; i++) {
                Bytes<Address> address = SortedCampaignNodes[forwardIndexes[i]];
                if (address == client.Address) continue;

                if (System.CampaignNodes[address] is SuperPeer peer) {
                    tasks.Add(client.P2P.SuperSend(peer, dict, data));
                }
            }
            try {
                await Task.WhenAll(tasks.ToArray());
            } catch { }
        }

        /// <summary>
        /// 将数据广播给其他超级节点
        /// </summary>
        /// <param name="data"></param>
        private void SuperBroadcast(string cmd, ReadOnlyMemory<byte> data) => SuperBroadcast(cmd, data, client.Address);

        private Task SuperSend(SuperPeer peer, string cmd, ReadOnlyMemory<byte> data) {
            BDict dict = new BDict { ["cmd"] = cmd, ["length"] = data.Length };
            return client.P2P.SuperSend(peer, dict, data);
        }

        private async ValueTask<Block?> TryGetBlock() {
            foreach (var (id, chipCollection) in blockChipCollectionDict) {
                if (chipCollection.CanRestore) {
                    try {
                        Block block = await chipCollection.RestoreAsync();
                        if (block.Height <= System.LastBlock.Height) continue;
                        return block;
                    } finally {
                        blockChipCollectionDict.Remove(id);
                    }
                }
            }
            return null;
        }

        private void System_StateTransferred(object? sender, SystemStateTransferredEventArgs e) {

        }



        public async void Start() {
            if (cancellationTokenSource is null) {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(client.CloseCancellationToken);
            } else {
                return;
            }


            client.P2P.SuperConnected += P2P_SuperConnected;

            client.Log("开始连接其他超级节点...");
            await ConnectAllSuperEndPoint();
            client.Log("连接其他超级节点完成");

            StartLoop();
        }

        static int __debug_ConnectCount = 0;

        /// <summary>
        /// 连接所有未连接的超级节点
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAllSuperEndPoint() {
            var tasks = new List<Task>();

            int count = Math.Min(Constants.MinProducerCount, SortedCampaignNodes.Length);
            for (int i = 0; i < count; i++) {
                Bytes<Address> addr = SortedCampaignNodes[i];
                if (addr == client.Address) continue;
                if (!System.CampaignNodes.TryGetValue(addr, out var peer) || peer is null) {
                    tasks.Add(Task.Run(async delegate {
                        if (await client.GetTargetNode(addr) is Node node) {
                            peer = new SuperPeer(new SuperNode(System.GetPublicKey(addr)!, node.IPEndPoint));
                            //Console.WriteLine($"连接.... {client.Address} => {addr}");
                            if (await client.P2P.SuperConnect(peer)) {
                                Interlocked.Increment(ref __debug_ConnectCount);
                                Console.WriteLine($"{__debug_ConnectCount,3} 连接完成 {client.Address} => {addr}");
                            } else {

                            }
                            System.CampaignNodes[addr] = peer;
                        }
                    }));
                }
            }

            try {
                await Task.WhenAll(tasks.ToArray());
            } catch {

            }
        }

        private async Task DownloadBlocks(uint minHeight, IEnumerable<IPEndPoint> eps) {
            foreach (IPEndPoint? ep in eps) {
                if (ep is null) continue;
                try {
                    await foreach (Block block in client.P2P.GetBlocks(ep, System.LastBlock.Height + 1)) {
                        Console.WriteLine($"下载：{client.Name} => [{System.LastBlock.Height + 1}], [{block.Height}]{block.Hash}");
                        System.VerifyExecuteBlock(block);
                        System.CommitBlock(block);
                    }
                    if (System.LastBlock.Height >= minHeight) return;
                } catch { }
            }
        }

        /// <summary>
        /// 从超级节点下载区块到最新区块
        /// </summary>
        /// <returns></returns>
        private async Task DownloadBlocks() {
            IPEndPoint[] eps = GetNextProducerEP().OfType<IPEndPoint>().ToArray();
            // eps.AsSpan().RandomShuffle();
            uint maxHeight = await client.P2P.GetBlockHeight(eps);
            await DownloadBlocks(maxHeight, eps);
        }

        /// <summary>
        /// 从超级节点下载区块到最新区块
        /// </summary>
        /// <returns></returns>
        private Task DownloadBlocks(uint minHeight) {
            return DownloadBlocks(minHeight, GetNextProducerEP().OfType<IPEndPoint>());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<挂起>")]
        private async void StartLoop() {
            Debug.Assert(cancellationTokenSource is not null);
            Debug.Assert(System.LastBlock.CommitState is not null);

            if (DateTime.UtcNow < BlockChainTimestamp.GenesisDateTime) {
                lastWaitTimer = Stopwatch.StartNew();
            }

            while (true) {
                try {
                    bool canProduce = DateTime.UtcNow >= BlockChainTimestamp.GenesisDateTime
                        && System.IsProducer(client.Address);
                    if (canProduce) break;

                    await Task.Delay(10, cancellationTokenSource.Token);
                } catch {
                    return;
                }
            }

            System.InProduction = true;

            Task Yield() => Task.Delay(1, cancellationTokenSource.Token);

            int maxVoteMissCount = Constants.MinProducerCount / 3;
            var precommitCounter = new DictionaryCounter<(Bytes<Hash256> HashPrevBlock, Bytes<Hash256> HashVote)>();
            var precommitTimer = new Stopwatch();
            var precommitAddressRecord = new HashSet<(Bytes<Address>, Bytes<Hash256>)>();
            var commitAddressRecord = new HashSet<Bytes<Address>>();
            var otherCommitBlocks = new List<Block>();

            Task synchronizeTask = Task.CompletedTask; // 某项同步任务。当存在未完成任务时，本超级节点会暂停出块，并且对其他超级节点生产的区块一律投否决票

            while (cancellationTokenSource.IsCancellationRequested is false) {
                precommitCounter.Clear();
                precommitTimer.Restart();

                bool validBlock = false; // 表示区块已验证通过
                Block? block = null;
                sbyte[] precommitFlags = new sbyte[Program.clientIndexes.Count];
                sbyte[] commitFlags = new sbyte[Program.clientIndexes.Count];

                try {
                SynchronizeState:
                    // Console.WriteLine($"lastWait {client.Address}: {lastWaitTimer.ElapsedMilliseconds}");
                    //client.Log($"lastWaitMilliseconds: {lastWaitTimer.ElapsedMilliseconds}");

                    if (synchronizeTask.IsCompleted) {
                        int commitCount = 0;
                        for (; commitCount < otherCommitBlocks.Count; commitCount++) {
                            if (otherCommitBlocks[commitCount].Height > System.LastBlock.Height)
                                break;
                        }

                        for (; commitCount < otherCommitBlocks.Count; commitCount++) {
                            Block commitBlock = otherCommitBlocks[commitCount];
                            if (commitBlock.Height == System.LastBlock.Height + 1) {
                                System.VerifyExecuteBlock(commitBlock);
                                System.CommitBlock(commitBlock);
                            } else {
                                synchronizeTask = DownloadBlocks(commitBlock.Height - 1);
                                break;
                            }
                        }

                        otherCommitBlocks.RemoveRange(0, commitCount);

                        if (block is null) {
                            IReadOnlyList<int>? forwardIndexes = GetNextProducers(client.Address);
                            if (forwardIndexes is not null) {
                                block = TryMakeBlock();

                                if (block is not null) {
                                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] <=> [{block.DateTime:HH:mm:ss.fff}]");
                                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 尝试出块：{client.Name} => [{block.Height}]{block.Hash}");

                                    int totalCount = forwardIndexes.Count - 1;
                                    int restoreCount = totalCount - totalCount / 3;
                                    int survive = 0;
                                    for (int i = 0; i < totalCount; i++) {
                                        if (System.CampaignNodes[SortedCampaignNodes[forwardIndexes[i]]] is SuperPeer { IsOnline: true }) {
                                            survive++;
                                        }
                                    }

                                    if (survive < restoreCount) {
                                        List<Task> connectTasks = new List<Task>();
                                        for (int i = 0; i < totalCount; i++) {
                                            if (System.CampaignNodes[SortedCampaignNodes[forwardIndexes[i]]] is SuperPeer { IsOnline: false } peer) {
                                                connectTasks.Add(client.P2P.SuperConnect(peer).ContinueWith(t => {
                                                    if (t.Result) {
                                                        Interlocked.Increment(ref survive);
                                                    }
                                                }));
                                            }
                                        }
                                        try {
                                            await Task.WhenAll(connectTasks.ToArray());
                                        } catch { }
                                    }

                                    if (survive < restoreCount) {
                                        block = null;
                                        // 根据Address请求超级节点IP和端口
                                        synchronizeTask = ConnectAllSuperEndPoint();
                                        try { goto SynchronizeState; } finally { await Yield(); }
                                    }

                                    BlockChip[] blockChips = BlockChip.Split(block, totalCount, restoreCount, PrivateKey);
                                    for (int i = 0; i < blockChips.Length; i++) {
                                        if (System.CampaignNodes[SortedCampaignNodes[forwardIndexes[i]]] is SuperPeer peer) {
                                            byte[] chipData = blockChips[i].Serialize();
                                            _ = SuperSend(peer, "producer_chip", chipData);
                                        }
                                    }
                                    validBlock = true;
                                }
                            }
                        }

                        if (block is null) {
                            block = await TryGetBlock();
                            validBlock = false;
                        }

                        if (block is not null) {
                            // 验证区块，修改 validBlock，投出precommit hash或precommit null

                            if (validBlock is false) {
                                CommitVote precommitVote;
                                try {
                                    System.VerifyExecuteBlock(block);
                                    validBlock = true;
                                    //precommitFlags[^1] = 1;
                                    precommitVote = CommitVote.Precommit(block.HashPrevBlock, block.Hash, PrivateKey);
                                } catch (InvalidBlockException) {
                                    //precommitFlags[^1] = -1;
                                    precommitVote = CommitVote.Precommit(block.HashPrevBlock, Bytes<Hash256>.Empty, PrivateKey);
                                }
                                SuperBroadcast("precommit_vote", precommitVote.Serialize());
                            }
                            precommitTimer.Restart();
                        }
                    } else {
                        if (block is null) {
                            block = await TryGetBlock();
                            validBlock = false;
                        }

                        if (block is not null) {
                            //precommitFlags[^1] = -1;
                            var precommitVote = CommitVote.Precommit(block.HashPrevBlock, Bytes<Hash256>.Empty, PrivateKey);
                            SuperBroadcast("precommit_vote", precommitVote.Serialize());
                        }

                        precommitTimer.Restart();
                    }


                    int commitHash = 0, commitNull = 0;
                    precommitAddressRecord.Clear();
                    commitAddressRecord.Clear();

                PrecommitState:
                    Bytes<Hash256> precommitHash = default, precommitPrevHash;

                    while (precommitVotes.TryTake(out var votePair)) {
                        if (block is null) { block = await TryGetBlock(); }
                        if (block is null) break;

                        var precommit = votePair.Vote;
                        if (precommitAddressRecord.Add((votePair.Node.Address, votePair.Vote.HashPrevBlock)) is false) {
                            continue; // 防止重复计票
                        }
                        //client.Log($"收到投票：{client.Name} => 来自[{votePair.Node.Address}] {precommit.HashVote}");
                        //Console.WriteLine($"收到投票：{client.Name} <= 来自[{Program.clientNames[votePair.Node.Address]}] {precommit.HashVote}");
                        int voteCount = ++precommitCounter[(precommit.HashPrevBlock, precommit.HashVote)];
                        if (precommit.HashVote == Bytes<Hash256>.Empty) {
                            //precommitFlags[Program.clientIndexes[votePair.Node.Address]] = -1;
                            if (voteCount >= maxVoteMissCount) {
                                // 转到commit投票阶段
                                precommitPrevHash = precommit.HashPrevBlock;
                                try { goto CommitState; } finally { await Yield(); }
                            }
                        } else {
                            //precommitFlags[Program.clientIndexes[votePair.Node.Address]] = 1;
                            if (voteCount >= Constants.MinProducerVoteAccpetCount) {
                                // 转到commit投票阶段
                                precommitPrevHash = precommit.HashPrevBlock;
                                precommitHash = precommit.HashVote;
                                try { goto CommitState; } finally { await Yield(); }
                            }
                        }
                    }

                    if (block is not null) {
                        if (precommitTimer.ElapsedMilliseconds > 800) {
                            block = null;
                            try { goto SynchronizeState; } finally { await Yield(); }
                        }
                        try { goto PrecommitState; } finally { await Yield(); }
                    } else {
                        try { goto SynchronizeState; } finally { await Yield(); }
                    }

                CommitState:
                    Bytes<Hash256> commitPrevHash, commitCurrHash;
                    CommitVote myCommitVote;
                    if (precommitHash != Bytes<Hash256>.Empty && precommitHash != block.Hash) {
                        while (true) {
                            block = await TryGetBlock();
                            if (block is null) break;
                            if (block.Hash == precommitHash) {
                                try {
                                    if (synchronizeTask.IsCompleted) {
                                        System.VerifyExecuteBlock(block);
                                        validBlock = true;
                                    }
                                } catch (InvalidBlockException) {
                                    block = null;
                                }
                                break;
                            }
                        }
                    }

                    if (block is not null && validBlock) {
                        //commitFlags[^1] = 1;
                        myCommitVote = CommitVote.Commit(block.HashPrevBlock, block.Hash, PrivateKey);
                    } else {
                        //commitFlags[^1] = -1;
                        myCommitVote = CommitVote.Commit(precommitPrevHash, Bytes<Hash256>.Empty, PrivateKey);
                    }
                    SuperBroadcast("commit_vote", myCommitVote.Serialize());


                    var commitTimer = Stopwatch.StartNew();
                    while (commitTimer.ElapsedMilliseconds <= 800) {
                        while (commitVotes.TryTake(out var votePair)) {
                            var vote = votePair.Vote;
                            if (vote.HashPrevBlock == precommitPrevHash) {
                                if (commitAddressRecord.Add(votePair.Node.Address) is false) {
                                    continue; // 防止重复计票
                                }

                                if (vote.HashVote == Bytes<Hash256>.Empty) {
                                    //commitFlags[Program.clientIndexes[votePair.Node.Address]] = -1;
                                    commitNull++;
                                    if (commitNull >= Constants.MinProducerVoteAccpetCount) {
                                        commitPrevHash = vote.HashPrevBlock;
                                        goto NoCommit;
                                    }
                                } else if (vote.HashVote == precommitHash) {
                                    //commitFlags[Program.clientIndexes[votePair.Node.Address]] = 1;
                                    commitHash++;
                                    if (commitHash >= Constants.MinProducerVoteAccpetCount) {
                                        commitPrevHash = vote.HashPrevBlock;
                                        commitCurrHash = vote.HashVote;
                                        goto CommitBlock;
                                    }
                                }
                            }
                        }
                        await Yield();
                    }

                NoCommit:
                    Logs($"提交 {client.Name} => null");
                    continue;

                CommitBlock:
                    if (block is null || block.Height >= System.LastBlock.Height + 2) {
                        if (synchronizeTask.IsCompleted) {
                            Console.WriteLine($"准备下载：{client.Name} => {commitCurrHash}");
                            if (block is not null && block.Height == System.LastBlock.Height + 2) {
                                // 整个系统确认了区块，但自己却没收到，向出块节点请求区块
                                synchronizeTask = Task.Run(async delegate {
                                    foreach (var ep in GetNextProducerEP()) {
                                        if (ep is null) continue;
                                        try {
                                            Block? downloadBlock = await client.P2P.GetBlockFromHeight(ep, System.LastBlock.Height + 1, onlyHeader: false);
                                            if (downloadBlock is not null) {
                                                System.VerifyExecuteBlock(downloadBlock);
                                                System.CommitBlock(downloadBlock);
                                                System.VerifyExecuteBlock(block);
                                                System.CommitBlock(block);
                                                return;
                                            }
                                        } catch { }
                                    }
                                    throw new Exception();
                                }).ContinueWith(t => {
                                    if (t.IsFaulted) {
                                        Console.WriteLine("2");
                                        synchronizeTask = DownloadBlocks();
                                    }
                                });
                            } else {
                                Console.WriteLine("3");
                                synchronizeTask = DownloadBlocks();
                            }
                        }
                        try { goto SynchronizeState; } finally { await Yield(); }
                    }

                    if (validBlock) {
                        myBlockTimer = null;
                        System.CommitBlock(block);
                        Logs($"提交 {client.Name} => [{block.Height}]{block.Hash}");


                        // TODO: 广播区块
                        if (block.ProducerAddress == client.Address) {

                        }
                    } else {
                        Console.WriteLine($"缓存区块：{client.Name} => [{block.Height}]{block.Hash}");
                        otherCommitBlocks.Add(block);
                    }

                    // 在未收满precommit null的时候，且precommit hash数量为0，那么继续等待区块和precommit
                    // 收满precommit null，转到commit投票阶段，投commit null
                    // 收满precommit hash但没有收到区块，转到commit投票阶段，投commit null
                    // 收到区块，验证通过，立刻投出precommit hash
                    // 收到区块，验证不通过，立刻投出precommit null

                    void Logs(string content) {
                        StringBuilder sb = new StringBuilder();
                        //if (block is null) {
                        //    sb.Append("---block: null");
                        //} else {
                        //    sb.Append($"---block: [{block.Height}]{block.Hash} [{validBlock}]");
                        //}
                        //sb.AppendLine();
                        //sb.AppendLine($"---{precommitHash}");
                        //sb.Append("---precommit: ").Append(FlagToChar(precommitFlags[^1])).Append('+').Append(FlagsToString(precommitFlags));
                        //sb.Append("  ");
                        //sb.Append("  commit: ").Append(FlagToChar(commitFlags[^1])).Append('+').Append(FlagsToString(commitFlags)).Append($" [{commitHash}]");
                        //sb.AppendLine();
                        sb.Append(content);
                        Console.WriteLine(sb);
                    }

                } catch {
                    continue;
                }

                lastWaitTimer = Stopwatch.StartNew();
                nextRoundBlockState = null;



            }

            static char FlagToChar(sbyte f) => f switch {
                -1 => '0',
                1 => '1',
                _ => '.',
            };

            static string FlagsToString(sbyte[] flags) {
                var chars = new char[flags.Length];
                for (int i = 0; i < flags.Length; i++) {
                    chars[i] = FlagToChar(flags[i]);
                }
                return new string(chars);
            }
        }

        public void Stop() {
            if (cancellationTokenSource is not null) {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;

                client.P2P.SuperConnected -= P2P_SuperConnected;
            }
            System.InProduction = false;
        }

        /// <summary>
        /// 判断本节点本轮是否还有出块机会，有则获得本节点与上一个区块的理论出块间隔
        /// </summary>
        /// <returns></returns>
        private TimeSpan? GetProduceTimeSpan() {
            Bytes<Address>[] sortedCampaignNodes = SortedCampaignNodes;
            int index = sortedCampaignNodes.AsSpan(0, Constants.MinProducerCount).IndexOf(client.Address);
            if (index < 0) throw new NoProductionAuthorityException();

            if (System.LastBlock.BigRound.IsValid is false) {
                return null;
            }

            int lastIndex = System.LastBlock.SmallRound.IndexInRound;
            if (index > lastIndex) {
                return (index - lastIndex) * Constants.ProduceTimeSpan;
            } else if (index == lastIndex) {
                return null;
            } else {
                if (System.LastBlock.SmallRound.Rounds % 3 == 0) {
                    return null;
                }
                return (index - lastIndex + Constants.MinProducerCount) * Constants.ProduceTimeSpan;
            }
        }

        /// <summary>
        /// 获得统计票数后的结果来计算本节点与上一个区块的理论出块间隔
        /// </summary>
        /// <returns></returns>
        private TimeSpan? GetNextRoundProduceTimeSpan() {
            Debug.Assert(nextRoundBlockState is { });

            Bytes<Address>[] sortedCampaignNodes = nextRoundBlockState.SortedCampaignNodes;
            int index = sortedCampaignNodes.AsSpan(0, Constants.MinProducerCount).IndexOf(client.Address);
            if (index < 0) return null;

            if (System.LastBlock.BigRound.IsValid is false) {
                return (index + 1) * Constants.ProduceTimeSpan;
            }

            int lastIndex = System.LastBlock.SmallRound.IndexInRound;
            return (Constants.MinProducerCount - lastIndex + index) * Constants.ProduceTimeSpan;
        }



        /// <summary>
        /// TODO: 尝试生产区块
        /// </summary>
        /// <returns></returns>
        unsafe public Block? TryMakeBlock() {
            Debug.Assert(System.LastBlock.CommitState is { });

            if (myBlockTimer is not null && myBlockTimer.Elapsed < Constants.MaxProducerCount * (Constants.ProduceTimeSpan - Constants.ProduceTimeOffset)) {
                return null;
            }

            TimeSpan? shouldTimeSpan = null; // 应等待的出块时间
            if (nextRoundBlockState is null) {
                shouldTimeSpan = GetProduceTimeSpan();
            }
            if (shouldTimeSpan is null) {
                if (nextRoundBlockState is null) {
                    nextRoundBlockState = BlockChainSystem.StatisticsCampaignNodes(System.LastBlock.CommitState);
                }
                shouldTimeSpan = GetNextRoundProduceTimeSpan();
            }
            if (shouldTimeSpan is null) {
                return null;
            }

            TimeSpan produceTimeSpan = shouldTimeSpan.Value;
            TimeSpan waitTimeSpan = lastWaitTimer?.Elapsed ?? Constants.ProduceTimeSpan;
            DateTime lastProduceTime = System.LastBlock.DateTime + waitTimeSpan;
            TimeSpan diffTimeSpan = DateTime.UtcNow - lastProduceTime; // 本地时间与出块时间的差

            const int Scale = 10;
            if (diffTimeSpan > Constants.ProduceTimeOffset * Scale) { // 出块过慢
                produceTimeSpan -= Constants.ProduceTimeOffset; // 因为出块过慢，减少应等待时间
            } else if (diffTimeSpan < -Constants.ProduceTimeOffset * Scale) { // 出块过快
                produceTimeSpan += Constants.ProduceTimeOffset; // 因为出块过块，增加应等待时间
            } else {
                produceTimeSpan -= diffTimeSpan / Scale;
            }

            if (waitTimeSpan < produceTimeSpan) return null; // 没有等待应等待时间，不出块

            BlockState precommitState;
            if (nextRoundBlockState is null) {
                precommitState = System.LastBlock.CommitState.NextNew();
            } else {
                precommitState = nextRoundBlockState;
                precommitState.WorldState = System.LastBlock.CommitState.WorldState.NextNew();
                precommitState.Transactions = new MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>>();
            }

            var newBlock = new Block() {
                Version = Constants.BlockVersion,
                Height = System.LastBlock.Height + 1,
                Timestamp = BlockChainTimestamp.ToTimestamp(System.LastBlock.DateTime + shouldTimeSpan.Value),
                HashPrevBlock = System.LastBlock.Hash,
                ProducerPublicKey = Secp256k1.Secp256k1.CreatePublicKey(PrivateKey),
                PrecommitState = precommitState
            };
            newBlock.ProducerAddress = newBlock.ProducerPublicKey.ToAddress();

            var transactions = new List<Transaction>();
            int bytes = 0;

            var rollbackTransactions = new List<Transaction>();
            while (transactionPool.Pop() is Transaction tx) {
                if (bytes + tx.Bytes >= Constants.MaxBlockBytes) {
                    transactionPool.Push(tx);
                    break;
                }

                if (tx.AttachData is VoteData vote && vote.Round > System.LastBlock.BigRound.Rounds) {
                    rollbackTransactions.Add(tx);
                    continue;
                }

                uint errorCode = 0;
                try {
                    System.ExecuteTransaction(newBlock, tx);
                } catch (ExecuteTransactionException e) {
                    errorCode = e.ErrorCode;
                } catch (InvalidTransactionException) {
                    continue;
                }

                transactions.Add(tx);
                newBlock.PrecommitState.Transactions.Add(tx.Hash, new TransactionResult(tx, errorCode));
                bytes += tx.Bytes;
            }

            foreach (var tx in rollbackTransactions) transactionPool.Push(tx);

            newBlock.Transactions = transactions.ToArray();

            newBlock.PrecommitState.Transactions.ComputeHash(BlockChainSystem.TransactionHashAlgorithm.Instance);
            newBlock.HashTxMerkleRoot = newBlock.PrecommitState.Transactions.RootHash;

            System.ProduceBlockReward(newBlock);

            newBlock.PrecommitState.WorldState.ComputeHash(BlockChainSystem.UserStateHashAlgorithm.Instance);
            newBlock.HashWorldState = newBlock.PrecommitState.WorldState.RootHash;

            newBlock.SignComputeHash(PrivateKey);

            if (myBlockTimer is null) myBlockTimer = new Stopwatch();
            myBlockTimer.Restart();

            nextRoundBlockState = null;
            return newBlock;
        }



        private bool isDisposed;

        private void Dispose(bool disposing) {
            if (!isDisposed) {
                Stop();

                if (disposing) {
                }

                isDisposed = true;
            }
        }

        ~ProducerSystem() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


    }
}
