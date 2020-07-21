#nullable enable

using OnlyChain.Model;
using OnlyChain.Network;
using OnlyChain.Network.Objects;
using OnlyChain.Secp256k1;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class ProducerSystem : IDisposable {
        static readonly TimeSpan TimeSpanRange = TimeSpan.FromMinutes(3); // 区块时间戳超过当前系统时间3分钟之外认为无效
        const int NextBlockMilliseconds = 1500; // 允许跨见证人的间隔时间
        const byte ProducerBlockChipPrefix = 1;
        const byte OtherBlockChipPrefix = 2;
        const byte PrecommitVoteTypePrefix = 3;
        const byte CommitVoteTypePrefix = 4;

        private readonly IClient client;
        private readonly BlockChainSystem system;
        private readonly SuperNodeClient superNodeClient;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly TransactionPool transactionPool = new TransactionPool(); // 交易池
        private readonly Blacklist<Hash<Size256>> blacklistTransactions = new Blacklist<Hash<Size256>>(Constants.BlacklistTxCount); // 交易黑名单
        private readonly ConcurrentDictionary<Hash<Size160>, BlockChipCollection> blockChipCollectionDict = new ConcurrentDictionary<Hash<Size160>, BlockChipCollection>();
        private readonly ConcurrentBag<CommitVote> precommitVotes = new ConcurrentBag<CommitVote>();
        private readonly ConcurrentBag<CommitVote> commitVotes = new ConcurrentBag<CommitVote>();
        private readonly Stopwatch lastWaitTimer = new Stopwatch();
        private BlockState? nextRoundBlockState = null;


        private DateTime lastDateTime = DateTime.Now; // 上一次收到区块的本地时间

        private byte[] PrivateKey => superNodeClient.PrivateKey;

        private Address[] SortedCampaignNodes => system.LastBlock.CommitState!.SortedCampaignNodes;

        public uint ProducerTimestamp {
            get {
                throw new NotImplementedException();
            }
        }


        public ProducerSystem(IClient client, SuperNodeClient superNodeClient) {
            this.client = client;
            system = client.System;
            this.superNodeClient = superNodeClient;

            system.StateTransferred += System_StateTransferred;
            superNodeClient.DataArrived += SuperNodeClient_DataArrived;
        }

        /// <summary>
        /// 根据当前出块人获取之后所有出块人的列表
        /// </summary>
        /// <param name="currentProducer"></param>
        /// <returns></returns>
        public IReadOnlyList<int>? GetNextProducers(Address currentProducer) {
            Debug.Assert(system.LastBlock.CommitState is { });

            BlockState state = system.LastBlock.CommitState;
            int index = Array.BinarySearch(state.SortedCampaignNodes, currentProducer, state.CampaignComparer);
            if (index < 0 || index >= Constants.MinProducerCount) return null;
            var result = new int[Constants.MinProducerCount];
            for (int i = 0; i < Constants.MinProducerCount; i++) {
                result[i] = (index + i + 1) % Constants.MinProducerCount;
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<挂起>")]
        private void SuperNodeClient_DataArrived(object? sender, SuperNodeEventArgs e) {
            Address address = e.SuperNode.PublicKey.ToAddress();

            if (e.Data.IsEmpty) return;
            if (SortedCampaignNodes.AsSpan(0, Constants.MinProducerCount).Contains(address) is false) return;



            try {
                switch (e.Data.Span[0]) {
                    case ProducerBlockChipPrefix:
                        ProducerBlockChipArrived(BlockChip.Parse(e.Data.Span[1..]), e.SuperNode.PublicKey, address);
                        SuperNodeBroadcast(MakeBroadcastPacket(OtherBlockChipPrefix, e.Data.Span[1..]), address);
                        break;
                    case OtherBlockChipPrefix:
                        OtherBlockChipArrived(BlockChip.Parse(e.Data.Span[1..]));
                        break;
                    case PrecommitVoteTypePrefix:
                        var precommitVote = new CommitVote(e.Data.Span[1..]);
                        if (!precommitVote.IsPrecommit) return;
                        if (Ecdsa.Verify(e.SuperNode.PublicKey, precommitVote.Hash, precommitVote.Signature) is false) return;
                        PrecommitVoteArrived(precommitVote);
                        break;
                    case CommitVoteTypePrefix:
                        var commitVote = new CommitVote(e.Data.Span[1..]);
                        if (commitVote.IsPrecommit) return;
                        if (Ecdsa.Verify(e.SuperNode.PublicKey, commitVote.Hash, commitVote.Signature) is false) return;
                        CommitVoteArrived(commitVote);
                        break;
                }
            } catch {
            }
        }

        private void ProducerBlockChipArrived(BlockChip blockChip, PublicKey producerPublicKey, Address producerAddress) {
            int beginIndex = 0;
            if (system.LastBlock.SmallRound.IsValid) {
                beginIndex = system.LastBlock.SmallRound.IndexInRound + 1;
            }
            int indexRange = Math.Clamp((int)Math.Floor((double)lastWaitTimer.ElapsedMilliseconds / NextBlockMilliseconds), 0, Constants.MinProducerCount);
            bool allowProduce = false;
            for (int i = 0; i < indexRange; i++) {
                if (SortedCampaignNodes[(beginIndex + i) % Constants.MinProducerCount] == producerAddress) {
                    allowProduce = true;
                    break;
                }
            }
            if (allowProduce is false) throw new InvalidBlockChipException();

            if (blockChipCollectionDict.TryAdd(blockChip.Id, new BlockChipCollection(blockChip, producerAddress, producerPublicKey)) is false) {
                throw new InvalidBlockChipException();
            }
        }

        private void OtherBlockChipArrived(BlockChip blockChip) {
            if (!blockChipCollectionDict.TryGetValue(blockChip.Id, out var blockChipCollection))
                throw new InvalidBlockChipException();

            blockChipCollection.Add(blockChip);
        }

        private void PrecommitVoteArrived(CommitVote precommitVote) {
            precommitVotes.Add(precommitVote);
        }

        private void CommitVoteArrived(CommitVote commitVote) {
            commitVotes.Add(commitVote);
        }

        private byte[] MakeBroadcastPacket(byte typePrefix, ReadOnlySpan<byte> chipData) {
            var buffer = new byte[1 + chipData.Length];
            buffer[0] = typePrefix;
            chipData.CopyTo(buffer.AsSpan(1));
            return buffer;
        }

        private async void SuperNodeBroadcast(ReadOnlyMemory<byte> data, Address beginAddress) {
            IReadOnlyList<int>? forwardIndexes = GetNextProducers(beginAddress);
            if (forwardIndexes is null) return;

            var tasks = new List<Task>();
            for (int i = 0; i < forwardIndexes.Count - 1; i++) {
                Address address = SortedCampaignNodes[forwardIndexes[i]];
                if (address == client.Address) continue;

                SuperNode? superNode = system.CampaignNodes[address];
                if (superNode is { } && superNode.Connected) {
                    tasks.Add(superNode.SendAsync(data, cancellationTokenSource.Token).AsTask());
                }
            }
            await Task.WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// 将数据广播给其他超级节点
        /// </summary>
        /// <param name="data"></param>
        private void SuperNodeBroadcast(ReadOnlyMemory<byte> data) => SuperNodeBroadcast(data, client.Address);


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<挂起>")]
        private async Task<Block?> GetBlockAsync(int timeoutMilliseconds) {
            Stopwatch? timer = null;
            if (timeoutMilliseconds > 0) {
                timer = new Stopwatch();
                timer.Start();
            }

            try {
                while (true) {
                    foreach (var (id, chipCollection) in blockChipCollectionDict) {
                        if (chipCollection.CanRestore) {
                            try {
                                return await chipCollection.RestoreAsync();
                            } finally {
                                blockChipCollectionDict.Remove(id, out _);
                            }
                        }
                    }

                    if (timer is { } && timer.ElapsedMilliseconds >= timeoutMilliseconds) {
                        break;
                    }

                    await Task.Delay(1, cancellationTokenSource.Token);
                }
            } catch {
            }
            return null;
        }

        private Block? TryGetBlock() {
            foreach (var (id, chipCollection) in blockChipCollectionDict) {
                if (chipCollection.CanRestore) {
                    try {
                        return chipCollection.RestoreAsync().Result;
                    } finally {
                        blockChipCollectionDict.Remove(id, out _);
                    }
                }
            }
            return null;
        }

        private void System_StateTransferred(object? sender, SystemStateTransferredEventArgs e) {

        }



        public void EnterProduction() {
            // TODO: 连接其他超级节点

            StartLoop();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<挂起>")]
        private async void StartLoop() {
            while (DateTime.Now < BlockChainTimestamp.GenesisDateTime) {
                try {
                    await Task.Delay(10, cancellationTokenSource.Token);
                } catch {
                    return;
                }
            }

            lastWaitTimer.Restart();

            Task Yield() => Task.Delay(1, cancellationTokenSource.Token);

            int maxVoteMissCount = Constants.MinProducerCount / 3;
            var precommitHashs = new Dictionary<Hash<Size256>, int>();
            var precommitTimer = new Stopwatch();

            while (cancellationTokenSource.IsCancellationRequested is false) {
                precommitHashs.Clear();
                int precommitNull = 0, precommitMiss = 0;
                int commitHash = 0, commitNull = 0;
                bool validBlock = false; // 表示区块已验证通过
                Block? block = null;
                bool noProduce = false; // 是否不共识，而处于同步状态
                Task synchronizeTask = Task.CompletedTask; // 正在同步区块的任务
                precommitTimer.Restart();

                try {
                    SynchronizeState:


                    if (noProduce) {
                        synchronizeTask = Task.Run(() => {

                        });

                        // TODO: 同步区块

                        noProduce = false;
                        continue;
                    }

                    if (synchronizeTask.IsCompleted) {
                        if (block is null) {
                            IReadOnlyList<int>? forwardIndexes = GetNextProducers(client.Address);
                            if (forwardIndexes is { }) {
                                block = TryMakeBlock();
                                if (block is { }) {
                                    int totalCount = forwardIndexes.Count - 1;
                                    int restoreCount = totalCount - totalCount / 3;
                                    int survive = 0;
                                    for (int i = 0; i < totalCount; i++) {
                                        if (system.CampaignNodes[SortedCampaignNodes[forwardIndexes[i]]] is SuperNode superNode && superNode.Connected) {
                                            survive++;
                                        }
                                    }
                                    if (survive < restoreCount) {
                                        block = null;
                                        try { goto SynchronizeState; } finally { await Yield(); }
                                    }

                                    BlockChip[] blockChips = BlockChip.Split(block, totalCount, restoreCount, PrivateKey);
                                    for (int i = 0; i < blockChips.Length; i++) {
                                        if (system.CampaignNodes[SortedCampaignNodes[forwardIndexes[i]]] is SuperNode superNode && superNode.Connected) {
                                            byte[] data = MakeBroadcastPacket(ProducerBlockChipPrefix, blockChips[i].Serialize());
                                            _ = superNode.SendAsync(data, cancellationTokenSource.Token);
                                        }
                                    }
                                    validBlock = true;
                                }
                            }
                        }

                        if (block is null) {
                            block = TryGetBlock();
                            validBlock = false;
                        }

                        if (block is { }) {
                            // 验证区块，修改 validBlock，投出precommit hash或precommit null

                            if (validBlock is false) {
                                CommitVote precommitVote;
                                try {
                                    system.VerifyExecuteBlock(block);
                                    validBlock = true;
                                    precommitVote = CommitVote.Precommit(system.LastBlock.Hash, block.Hash, PrivateKey);
                                } catch (InvalidBlockException) {
                                    precommitVote = CommitVote.Precommit(system.LastBlock.Hash, Hash<Size256>.Empty, PrivateKey);
                                }
                                SuperNodeBroadcast(MakeBroadcastPacket(PrecommitVoteTypePrefix, precommitVote.Serialize()));
                            }
                            precommitTimer.Restart();
                        }
                    } else {
                        precommitTimer.Restart();
                    }

                    PrecommitState:
                    Hash<Size256> precommitHash = default;

                    foreach (var precommit in precommitVotes) {
                        if (precommit.HashPrevBlock == system.LastBlock.Hash) {
                            if (precommit.HashVote == Hash<Size256>.Empty) {
                                precommitNull++;
                                if (precommitNull >= Constants.MinProducerVoteAccpetCount) {
                                    // 转到commit投票阶段
                                    try { goto CommitState; } finally { await Yield(); }
                                }
                            } else {
                                precommitHashs[precommit.HashVote]++;
                                if (precommitHashs[precommit.HashVote] >= Constants.MinProducerVoteAccpetCount) {
                                    // 转到commit投票阶段
                                    precommitHash = precommit.HashVote;
                                    try { goto CommitState; } finally { await Yield(); }
                                }
                            }
                        } else {
                            precommitMiss++;
                            if (precommitMiss > maxVoteMissCount) {
                                // 跟其他超级节点不在同一频道，退出共识，进入同步状态
                                noProduce = true;
                                try { goto SynchronizeState; } finally { await Yield(); }
                            }
                        }
                    }

                    if (block is { } || synchronizeTask.IsCompleted is false) {
                        if (precommitTimer.ElapsedMilliseconds > 500) {
                            try { goto CommitState; } finally { await Yield(); }
                        }
                        try { goto PrecommitState; } finally { await Yield(); }
                    } else {
                        try { goto SynchronizeState; } finally { await Yield(); }
                    }

                    CommitState:
                    CommitVote myCommitVote;
                    if (block is { } && validBlock && precommitHash == block.Hash) {
                        myCommitVote = CommitVote.Commit(block.HashPrevBlock, block.Hash, PrivateKey);
                    } else {
                        myCommitVote = CommitVote.Commit(system.LastBlock.HashPrevBlock, Hash<Size256>.Empty, PrivateKey);
                    }
                    SuperNodeBroadcast(MakeBroadcastPacket(CommitVoteTypePrefix, myCommitVote.Serialize()));


                    var commitTimer = Stopwatch.StartNew();
                    while (commitTimer.ElapsedMilliseconds <= 500) {
                        foreach (var vote in commitVotes) {
                            if (vote.HashPrevBlock == system.LastBlock.Hash) {
                                if (vote.HashVote == precommitHash) {
                                    commitHash++;
                                    if (commitHash >= Constants.MinProducerVoteAccpetCount) {
                                        goto CommitBlock;
                                    }
                                } else if (vote.HashVote == Hash<Size256>.Empty) {
                                    commitNull++;
                                    if (commitNull >= Constants.MinProducerVoteAccpetCount) {
                                        goto NoCommit;
                                    }
                                }
                            }
                        }
                        await Yield();
                    }

                    NoCommit:
                    continue;

                    CommitBlock:
                    if (block is null) {
                        noProduce = true;
                        try { goto SynchronizeState; } finally { await Yield(); }
                    }

                    system.CommitBlock(block);

                    // TODO: 广播区块


                    // 在未收满precommit null的时候，且precommit hash数量为0，那么继续等待区块和precommit
                    // 收满precommit null，转到commit投票阶段，投commit null
                    // 收满precommit hash但没有收到区块，转到commit投票阶段，投commit null
                    // 收到区块，验证通过，立刻投出precommit hash
                    // 收到区块，验证不通过，立刻投出precommit null



                } catch {
                    continue;
                }

                lastWaitTimer.Restart();
                nextRoundBlockState = null;
            }
        }

        /// <summary>
        /// 判断本节点本轮是否还有出块机会，有则获得本节点与上一个区块的理论出块间隔
        /// </summary>
        /// <returns></returns>
        private TimeSpan? GetProduceTimeSpan() {
            Address[] sortedCampaignNodes = SortedCampaignNodes;
            int index = sortedCampaignNodes.AsSpan(0, Constants.MinProducerCount).IndexOf(client.Address);
            if (index < 0) throw new NoProductionAuthorityException();

            if (system.LastBlock.BigRound.IsValid is false) {
                return null;
            }

            int lastIndex = system.LastBlock.SmallRound.IndexInRound;
            if (index > lastIndex) {
                return (index - lastIndex) * Constants.ProduceTimeSpan;
            } else if (index == lastIndex) {
                return null;
            } else {
                if (system.LastBlock.SmallRound.Rounds % 3 == 0) {
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

            Address[] sortedCampaignNodes = nextRoundBlockState.SortedCampaignNodes;
            int index = sortedCampaignNodes.AsSpan(0, Constants.MinProducerCount).IndexOf(client.Address);
            if (index < 0) return null;

            if (system.LastBlock.BigRound.IsValid is false) {
                return (index + 1) * Constants.ProduceTimeSpan;
            }

            int lastIndex = system.LastBlock.SmallRound.IndexInRound;
            return (Constants.MinProducerCount - lastIndex + index) * Constants.ProduceTimeSpan;
        }

        /// <summary>
        /// TODO: 尝试生产区块
        /// </summary>
        /// <returns></returns>
        public Block? TryMakeBlock() {
            Debug.Assert(system.LastBlock.CommitState is { });

            TimeSpan? shouldTimeSpan = null; // 应等待的出块时间
            if (nextRoundBlockState is null) {
                shouldTimeSpan = GetProduceTimeSpan();
            }
            if (shouldTimeSpan is null) {
                if (nextRoundBlockState is null) {
                    nextRoundBlockState = BlockChainSystem.StatisticsCampaignNodes(system.LastBlock.CommitState);
                }
                shouldTimeSpan = GetNextRoundProduceTimeSpan();
            }
            if (shouldTimeSpan is null) {
                return null;
            }

            TimeSpan produceTimeSpan = shouldTimeSpan.Value;
            TimeSpan waitTimeSpan = lastWaitTimer.Elapsed;
            DateTime lastProduceTime = system.LastBlock.DateTime + waitTimeSpan;
            TimeSpan diffTimeSpan = DateTime.UtcNow - lastProduceTime; // 本地时间与出块时间的差

            if (diffTimeSpan > Constants.ProduceTimeOffset) { // 出块过慢
                produceTimeSpan -= Constants.ProduceTimeOffset; // 因为出块过慢，减少应等待时间
            } else if (diffTimeSpan < -Constants.ProduceTimeOffset) { // 出块过快
                produceTimeSpan += Constants.ProduceTimeOffset; // 因为出块过块，增加应等待时间
            } else {
                produceTimeSpan -= diffTimeSpan;
            }

            if (waitTimeSpan < produceTimeSpan) return null; // 没有等待应等待时间，不出块

            BlockState precommitState;
            if (nextRoundBlockState is null) {
                precommitState = system.LastBlock.CommitState.NextNew();
            } else {
                precommitState = nextRoundBlockState;
                precommitState.WorldState = system.LastBlock.CommitState.WorldState.NextNew();
                precommitState.Transactions = new MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>>(0);
            }

            var newBlock = new Block() {
                Version = Constants.BlockVersion,
                Height = system.LastBlock.Height + 1,
                Timestamp = BlockChainTimestamp.ToTimestamp(system.LastBlock.DateTime + shouldTimeSpan.Value),
                HashPrevBlock = system.LastBlock.Hash,
                PrecommitState = precommitState
            };

            var transactions = new List<Transaction>();
            int bytes = 0;

            var rollbackTransactions = new List<Transaction>();
            while (transactionPool.Pop() is Transaction tx) {
                if (bytes + tx.Bytes >= Constants.MaxBlockBytes) {
                    transactionPool.Push(tx);
                    break;
                }

                if (tx.AttachData is VoteData vote && vote.Round > system.LastBlock.BigRound.Rounds) {
                    rollbackTransactions.Add(tx);
                    continue;
                }

                uint errorCode = 0;
                try {
                    system.ExecuteTransaction(newBlock, tx);
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

            system.ProduceBlockReward(newBlock);

            newBlock.PrecommitState.WorldState.ComputeHash(BlockChainSystem.UserStateHashAlgorithm.Instance);
            newBlock.HashWorldState = newBlock.PrecommitState.WorldState.RootHash;

            Serializer serializer = stackalloc byte[Block.NetworkMaxHeaderBytes()];
            newBlock.NetworkSerializeHeaderWithoutSignature(ref serializer);
            newBlock.Signature = Ecdsa.Sign(serializer, PrivateKey, out newBlock.HashSignHeader);
            serializer.Write(Serializer.Signature, newBlock.Signature);
            newBlock.Hash = serializer.RawData.MessageHash();
            nextRoundBlockState = null;
            return newBlock;
        }



        private bool isDisposed;

        private void Dispose(bool disposing) {
            if (!isDisposed) {
                cancellationTokenSource.Cancel();
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
