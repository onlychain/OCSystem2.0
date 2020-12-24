#nullable enable

using OnlyChain.Database;
using OnlyChain.Model;
using OnlyChain.Network;
using OnlyChain.Secp256k1;
using OnlyChain.Secp256k1.Math;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class BlockChainSystem : IDisposable {
        const int CacheHeight = 15000; // 最大缓存数量

        static readonly Regex wordRegex = new Regex(@"^\w+$", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly IClient client;

        private readonly BlockChainDatabase db;
        private readonly IndexedQueue<Block> cacheBlocks = new IndexedQueue<Block>(CacheHeight); // 缓存最近的区块
        private readonly Channel<Block> blockChannel = Channel.CreateUnbounded<Block>(new UnboundedChannelOptions { SingleReader = true });
        private readonly SortedSet<Block> tempNetworkBlocks = new SortedSet<Block>(); // 缓存来自网络的区块，按高度从小到大排序，高度不一定是连续的
        private ImmutableDictionary<Bytes<Address>, byte[]> allContracts = ImmutableDictionary<Bytes<Address>, byte[]>.Empty; // 所有合约代码
        private readonly object downloadBlocksLock = new object();
        private CancellationTokenSource downloadCancellationSource = null!;
        private uint downloadHeight = 0;



        public string ChainName { get; }

        public Block LastBlock { get; private set; } = null!;
        public ConcurrentDictionary<Bytes<Address>, SuperPeer?> CampaignNodes { get; private set; } = new(); // 所有竞选节点与IP端口(TCP)
        public BlockChainDatabase DB => db;
        public Task ReadDatabaseTask { get; }
        public Task DownloadBlocksTask { get; internal set; } = Task.CompletedTask;

        public bool InProduction { get; internal set; } = false;


        public event EventHandler<CampaignNodesChangedEventArgs>? CampaignNodesChanged;
        public event EventHandler<SystemStateTransferredEventArgs>? StateTransferred;


        public BlockChainSystem(IClient client, string databasePath, string chainName = "main") {
            this.client = client;

            ChainName = chainName ?? throw new ArgumentNullException(nameof(chainName));
            if (!wordRegex.IsMatch(chainName)) throw new ArgumentOutOfRangeException(nameof(chainName), "非法的链名");

            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{databasePath}-{chainName}");
            // string dataDir = Path.Combine("Z:", $"{databasePath}-{chainName}");

            db = new BlockChainDatabase(Path.Combine(dataDir, "blocks"));

            ReadDatabaseTask = Task.Run(() => {
                InitSystem();
                LoopReadBlock();
            });
        }

        [SuppressMessage("Style", "IDE0059:不需要赋值", Justification = "<挂起>")]
        private void InitSystem() {
            Block genesisBlock = BuildGenesisBlock();
            CommitBlock(genesisBlock);

            bool repair = false;

            for (uint h = 2; db.GetBlock(h) is Block block;) {
                try {
                    try { VerifyExecuteBlock(block); } catch { throw new BadDatabaseException(); }
                    CommitBlock(block, updateDatabase: false);
                    h++;
                } catch (BadDatabaseException) when (!repair) {
                    db.Repair();
                    repair = true;
                }
            }
        }

        private async void LoopReadBlock() {
            try {
                var sortedBlocks = new SortedSet<Block>();
                uint beginHeight = LastBlock.Height + 1;

                await foreach (var block in blockChannel.Reader.ReadAllAsync(client.CloseCancellationToken)) {
                    if (block.Height <= LastBlock.Height) continue;

                    sortedBlocks.Add(block);

                    while (sortedBlocks.Min is Block currBlock) {
                        if (currBlock.Height != LastBlock.Height + 1) break;
                        sortedBlocks.Remove(currBlock);

                        try {
                            VerifyExecuteBlock(currBlock);
                            CommitBlock(currBlock);
                        } catch { break; }
                    }

                }
            } catch (OperationCanceledException) { }
        }

        public ValueTask PutBlockFromNetwork(Block block) {
            if (InProduction) throw new InvalidOperationException("生产中不能从网络下载区块");

            if (block.Height <= LastBlock.Height) return ValueTask.CompletedTask;

            return blockChannel.Writer.WriteAsync(block, client.CloseCancellationToken);
        }

        private async Task CreateDownloadBlocksTask(int requestCount, IPEndPoint? remote) {
            if (requestCount <= 0) throw new ArgumentOutOfRangeException(nameof(requestCount));

            int currCount = 0;

            if (remote is not null) {
                if (await Download(remote)) return;
            }

            foreach (IPEndPoint ep in client.Nodes.FindNode(Bytes<Address>.Random()).Select(n => n.IPEndPoint)) {
                if (ep.Equals(remote)) continue;
                if (await Download(ep)) return;
            }



            async ValueTask<bool> Download(IPEndPoint ep) {
                try {
                    await foreach (Block block in client.P2P.GetBlocks(ep, LastBlock.Height + 1, onlyHeader: false, downloadCancellationSource.Token)) {
                        VerifyExecuteBlock(block);
                        CommitBlock(block);
                    }
                    if (LastBlock.Height >= downloadHeight || ++currCount >= requestCount) return true;
                } catch {
                    if (downloadCancellationSource.IsCancellationRequested) {
                        return true;
                    }
                }
                return false;
            }
        }

        internal Task DownloadBlocks(uint downloadHeight = uint.MaxValue, int requestCount = 10, IPEndPoint? remote = null) {
            if (InProduction is false) {
                lock (downloadBlocksLock) {
                    if (DownloadBlocksTask.IsCompleted is false) {
                        if (this.downloadHeight < downloadHeight) {
                            downloadCancellationSource.Cancel();
                        } else {
                            return DownloadBlocksTask;
                        }
                    }
                    this.downloadHeight = downloadHeight;
                    downloadCancellationSource = new CancellationTokenSource();
                    DownloadBlocksTask = CreateDownloadBlocksTask(requestCount, remote);
                }
            }
            return DownloadBlocksTask;
        }

        /// <summary>
        /// 验证执行区块，并设置<see cref="Block.PrecommitState"/>字段（执行前<see cref="Block.PrecommitState"/>字段必须为null）。
        /// </summary>
        /// <param name="block">非创世区块。</param>
        unsafe public void VerifyExecuteBlock(Block block) {
            if (LastBlock.CommitState is null) throw new InvalidOperationException();

            if (LastBlock.Hash != block.HashPrevBlock || LastBlock.Height != block.Height - 1)
                throw new InvalidBlockException();

            block.PrecommitState = LastBlock.CommitState.NextNew();

            bool verifyValid = true;
            var verifyTasks = new List<Task>();

            // 验证生产者
            if (!IsProducer(block.ProducerAddress)) throw new InvalidBlockException();

            // 验证签名，验证交易mpt
            verifyTasks.Add(Task.Run(delegate {
                block.ProducerPublicKey = Ecdsa.RecoverPublicKey(block.HashSignHeader, block.Signature);
                if (block.ProducerPublicKey.ToAddress() != block.ProducerAddress) {
                    verifyValid = false;
                }
            }));

            // 并行验证交易签名
            int taskCount = Math.Max(Environment.ProcessorCount - 2, 1);
            var perCount = (int)Math.Ceiling(block.Transactions.Length / (double)taskCount);
            int counter = 0;
            while (counter < block.Transactions.Length) {
                int processCount = Math.Min(perCount, block.Transactions.Length - counter);
                int startIndex = counter;
                verifyTasks.Add(Task.Run(delegate {
                    for (int i = 0; verifyValid && i < processCount; i++) {
                        Transaction transaction = block.Transactions[i + startIndex];
                        transaction.FromPublicKey = Ecdsa.RecoverPublicKey(transaction.HashSignHeader, transaction.Signature);
                        if (transaction.FromPublicKey.ToAddress() != transaction.From) {
                            verifyValid = false;
                        }
                    }
                }));

                counter += processCount;
            }

            try {
                // 当区块跨过轮次，则统计票数
                TryStatisticsCampaignNodes(block);

                // 执行交易
                var txMPT = block.PrecommitState.Transactions;
                for (int i = 0; i < block.Transactions.Length; i++) {
                    if (verifyValid is false) throw new InvalidBlockException();

                    Transaction tx = block.Transactions[i];
                    uint errorCode = 0;
                    try {
                        ExecuteTransaction(block, tx);
                    } catch (ExecuteTransactionException e) {
                        errorCode = e.ErrorCode;
                    }
                    txMPT.Add(tx.Hash, new TransactionResult(tx, errorCode));
                }

                // 验证交易MPT
                txMPT.ComputeHash(TransactionHashAlgorithm.Instance);
                if (block.HashTxMerkleRoot != txMPT.RootHash) throw new InvalidBlockException();

                // 出块奖励，设置世界MPT
                ProduceBlockReward(block);

                // 验证世界状态
                block.PrecommitState.WorldState.ComputeHash(UserStateHashAlgorithm.Instance);
                if (block.HashWorldState != block.PrecommitState.WorldState.RootHash) throw new InvalidBlockException();

                // 等待所有交易签名验证完成
                Task.WaitAll(verifyTasks.ToArray());
                if (verifyValid is false) throw new InvalidBlockException();
            } catch (InvalidBlockException) {
                throw;
            } catch {
                throw new InvalidBlockException();
            }
        }

        /// <summary>
        /// TODO: 出块奖励
        /// </summary>
        /// <param name="block"></param>
        internal void ProduceBlockReward(Block block) {

        }

        /// <remarks>
        /// <para>1、检查交易合法性</para>
        /// <para>2、执行交易（更新<see cref="Block.PrecommitState"/>）</para>
        /// </remarks>
        /// <param name="block">一个未检查执行过的区块</param>
        /// <param name="tx"></param>
        unsafe internal void ExecuteTransaction(Block block, Transaction tx) {
            try {
                Debug.Assert(block.PrecommitState is { });

                if ((ulong)tx.GasPrice is 0) goto Invalid; // 手续费不能为0
                if (tx.GasLimit >= long.MaxValue) goto Invalid;
                if (tx.GasLimit < tx.BaseGasUsed) goto Invalid; // GasLimit比最低汽油费还低
                if (!MathTools.CheckMul(tx.GasLimit, tx.GasPrice, out ulong maxFee)) goto Invalid;

                var mpt = block.PrecommitState.WorldState;
                if (!mpt.TryGetValue(tx.From, out UserState fromState)) goto Invalid;
                if (fromState.NextNonce != tx.Nonce) goto Invalid; // nonce不正确
                if (!MathTools.CheckMul(tx.BaseGasUsed, tx.GasPrice, out ulong minFee)) goto Invalid;

                if (fromState.Locks is { }) {
                    int unlockCount = 0;
                    foreach (var @lock in fromState.Locks) {
                        if (block.Timestamp < @lock.UnlockTimestamp) break; // fromStatus.Locks是排序的，因此可以直接跳出循环
                        fromState.Balance += @lock.Value;
                        unlockCount++;
                    }
                    if (unlockCount > 0) {
                        if (unlockCount != fromState.Locks.Length) {
                            fromState.Locks = fromState.Locks[unlockCount..];
                        } else {
                            fromState.Locks = null;
                        }
                    }
                }

                if (fromState.Balance < maxFee) goto Invalid;
                if (fromState.Balance < minFee) goto Invalid; // 不足以支付最低手续费

                UserState producerState = mpt[block.ProducerAddress];
                fromState.Balance -= minFee; // 扣除最低手续费
                producerState.Balance += minFee;

                UserState rollbackFromState = fromState; // 保存当前状态，交易执行失败时回滚
                void Fail(TransactionErrorCode errorCode) {
                    mpt[block.ProducerAddress] = producerState;
                    SetFromState(rollbackFromState);
                    throw new ExecuteTransactionException((uint)errorCode);
                }

                void SetFromState(in UserState state) {
                    bool remove = (ulong)state.Balance is 0
                        && (ulong)state.VotePledge is 0
                        && (ulong)state.SuperPledge is 0
                        && state.Votes is 0
                        && state.Locks is null
                        && state.ContractMemory is null;

                    if (remove) {
                        mpt.Remove(tx.From);
                    } else {
                        mpt[tx.From] = state;
                    }
                }

                if (fromState.Balance < tx.Value) Fail(TransactionErrorCode.InsufficientBalance); // 余额不足，但足够支付手续费
                fromState.Balance -= tx.Value;
                fromState.NextNonce++;

                switch (tx.AttachData) {
                    case null: { // 普通交易
                        if (mpt.TryGetValue(tx.To, out UserState toState)) {
                            toState.Balance += tx.Value;
                            mpt[tx.To] = toState;
                        } else {
                            mpt[tx.To] = new UserState { Balance = tx.Value };
                        }
                        break;
                    }
                    case SuperPledgeData: // 超级节点质押，本轮结束其他账户才能对自己投票
                        if ((ulong)fromState.SuperPledge is 0 && tx.Value < Constants.MinSuperPledgeCoin) Fail(TransactionErrorCode.PledgeTooLow); // 质押金额太小
                        if (fromState.SuperPledge >= Constants.MinSuperPledgeCoin && tx.Value < Constants.MinSuperPledgeIncrement) Fail(TransactionErrorCode.PledgeTooLow); // 质押金额太小
                        fromState.SuperPledge += tx.Value;
                        fromState.SuperPledgeTimestamp = block.Timestamp;
                        block.PrecommitState.TempCampaignNodes.Add(tx.From);
                        break;
                    case SuperRedemptionData: // 超级节点赎回
                        if ((ulong)tx.Value != 0) Fail(TransactionErrorCode.ValueNotEqualToZero); // Value不为0
                        if (fromState.SuperPledge < Constants.MinSuperPledgeCoin) Fail(TransactionErrorCode.Unpledged); // 从未质押过
                        if (block.Timestamp - fromState.SuperPledgeTimestamp < Constants.YearSeconds) Fail(TransactionErrorCode.NotExpired); // 未到赎回期
                        fromState.Balance += fromState.SuperPledge;
                        fromState.SuperPledge = 0;
                        fromState.SuperPledgeTimestamp = 0;
                        break;
                    case VoteData vote: // 投票
                        if (vote.Round != block.BigRound.Rounds) goto Invalid;
                        if (tx.Value < Constants.MinVotePledgeCoin) Fail(TransactionErrorCode.VoteTooLow);
                        if (vote.Addresses.Length != new HashSet<Bytes<Address>>(vote.Addresses).Count) Fail(TransactionErrorCode.DuplicateAddress); // 地址重复

                        var changedStatus = new Dictionary<Bytes<Address>, UserState>(60);

                        if (fromState.VoteAddresses is { }) {
                            foreach (var addr in fromState.VoteAddresses) {
                                UserState campaignStatus = mpt[addr];
                                campaignStatus.Votes -= fromState.VotePledge;
                                changedStatus[addr] = campaignStatus;
                            }
                        }

                        fromState.VotePledge += tx.Value;
                        fromState.VoteAddresses = vote.Addresses;
                        fromState.VotePledgeTimestamp = block.Timestamp;

                        foreach (Bytes<Address> addr in vote.Addresses) {
                            int index = Array.BinarySearch(block.PrecommitState.SortedCampaignNodes, addr, block.PrecommitState.CampaignComparer);
                            if (index < 0) Fail(TransactionErrorCode.Unpledged); // 投给非竞选节点
                            if (mpt.TryGetValue(addr, out UserState campaignStatus) is false) Fail(TransactionErrorCode.Unpledged);
                            if (IsCampaignNode(block, in campaignStatus)) Fail(TransactionErrorCode.Unpledged);
                            if (changedStatus.TryGetValue(addr, out UserState tempStatus)) campaignStatus = tempStatus;
                            campaignStatus.Votes += fromState.VotePledge;
                            changedStatus[addr] = campaignStatus;
                        }

                        foreach (var (addr, status) in changedStatus) mpt[addr] = status;
                        break;
                    case VoteRedemptionData: // 投票质押赎回
                        if ((ulong)tx.Value != 0) Fail(TransactionErrorCode.ValueNotEqualToZero); // Value不为0
                        if (fromState.VoteAddresses is null) Fail(TransactionErrorCode.Unpledged); // 从未质押过
                        if (block.Timestamp - fromState.VotePledgeTimestamp < Constants.VotePledgeSeconds) Fail(TransactionErrorCode.NotExpired);

                        for (int i = 0; i < fromState.VoteAddresses!.Length; i++) {
                            Bytes<Address> addr = fromState.VoteAddresses[i];
                            UserState campaignStatus = mpt[addr];
                            campaignStatus.Votes -= fromState.VotePledge;
                            mpt[addr] = campaignStatus;
                        }

                        fromState.Balance += fromState.VotePledge;
                        fromState.VoteAddresses = null;
                        fromState.VotePledge = 0;
                        fromState.VotePledgeTimestamp = 0;
                        break;
                    case LockData lockData: { // 锁仓
                        if (lockData.UnlockTimestamp <= block.Timestamp) goto case null; // 区块时间戳已过了解锁时间戳

                        var lockStatus = new LockStatus { Value = tx.Value, UnlockTimestamp = lockData.UnlockTimestamp };
                        if (mpt.TryGetValue(tx.To, out UserState toState)) {
                            var locks = toState.Locks ?? Array.Empty<LockStatus>();
                            var newLocks = new LockStatus[locks.Length + 1];
                            int insertIndex = Array.FindIndex(locks, o => o.UnlockTimestamp >= lockData.UnlockTimestamp);
                            if (insertIndex < 0) {
                                locks.CopyTo(newLocks, 0);
                                newLocks[^1] = lockStatus;
                            } else {
                                locks.AsSpan(0, insertIndex).CopyTo(newLocks);
                                newLocks[insertIndex] = lockStatus;
                                locks.AsSpan(insertIndex).CopyTo(newLocks.AsSpan(insertIndex + 1));
                            }
                            toState.Locks = newLocks;
                            mpt[tx.To] = toState;
                        } else {
                            mpt[tx.To] = new UserState { Locks = new[] { lockStatus } };
                        }
                        break;
                    }
                    case ContractInputData contractData: {
                        bool isCreate = tx.To == Bytes<Address>.Empty;
                        var tempStates = new Dictionary<Bytes<Address>, TempState>();
                        Bytes<Address> contractAddress;
                        ReadOnlySpan<byte> contractBytecode, contractInput;

                        tempStates[tx.From] = TempState.User(fromState.Balance); // 发送者一定不是合约地址

                        if (isCreate) {
                            Span<byte> buf = stackalloc byte[sizeof(Address) + sizeof(ulong)];
                            tx.From.CopyTo(buf[..sizeof(Address)]);
                            BinaryPrimitives.WriteUInt64BigEndian(buf[sizeof(Address)..], tx.Nonce);

                            contractBytecode = contractData.Input.Span;
                            contractInput = ReadOnlySpan<byte>.Empty;
                            contractAddress = ((ReadOnlySpan<byte>)buf).KeyToAddress();
                            tempStates[contractAddress] = TempState.Contract(0);
                        } else {
                            ref readonly UserState contractState = ref mpt.GetRefValue(tx.To);
                            if (contractState.IsNull() || contractState.ContractMemory is null) {
                                Fail(TransactionErrorCode.ContractNotFound);
                                return;
                            }

                            contractBytecode = allContracts[tx.To];
                            contractInput = contractData.Input.Span;
                            contractAddress = tx.To;
                            tempStates[contractAddress] = TempState.Contract(contractState.Balance);
                        }

                        var @interface = new ContractNative.evmc_host_interface();
                        var context = new ContractContext(block, tx, tempStates, mpt) { ValueLeft = tx.Value };
                        InitHostInterface(&@interface, context);

                        bool executeSuccess = false;
                        ulong gasLeft = unchecked(tx.GasLimit - tx.BaseGasUsed);
                        ulong gasUsed = 0;

                        fixed (byte* bytecode = contractBytecode)
                        fixed (byte* input = contractInput) {
                            var msg = new ContractNative.evmc_message {
                                Gas = unchecked((long)gasLeft),
                                Destination = contractAddress,
                                Sender = tx.From,
                                Value = new Bytes<U256>(tx.Value),
                                InputData = input,
                                InputSize = (nuint)contractInput.Length,
                            };
                            var result = ContractNative.VM->Execute(ContractNative.VM, &@interface, null, ContractNative.EVMC_MAX_REVISION, &msg, bytecode, (nuint)contractBytecode.Length);

                            if (result.StatusCode == ContractNative.evmc_status_code.EVMC_SUCCESS) {
                                if (isCreate) {
                                    byte[] runtimeBytecode = new ReadOnlySpan<byte>(result.OutputData, (int)result.OutputSize).ToArray();
                                    allContracts = allContracts.Add(contractAddress, runtimeBytecode);
                                }
                                executeSuccess = true;
                            }

                            gasUsed = gasLeft - result.GasLeft;

                            result.Release?.Invoke(&result);
                        }

                        if (executeSuccess) {
                            foreach (var (addr, newState) in tempStates) {
                                mpt.TryGetValue(addr, out var oldState);
                                bool change = newState.SelfDestruct;

                                if (oldState.Balance != newState.Balance) {
                                    oldState.Balance = newState.Balance;
                                    change = true;
                                }

                                if (newState.ContractMemory is { Count: not 0 }) {
                                    MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>> contractMemory;
                                    if (oldState.ContractMemory is null) {
                                        contractMemory = new MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>>();
                                    } else {
                                        contractMemory = oldState.ContractMemory.NextNew();
                                    }

                                    bool memoryChange = false;
                                    foreach (var (key, newValue) in newState.ContractMemory) {
                                        contractMemory.TryGetValue(key, out var oldValue);
                                        if (newValue != oldValue) {
                                            if (newValue != default) {
                                                contractMemory[key] = newValue;
                                            } else {
                                                contractMemory.Remove(key);
                                            }
                                            memoryChange = true;
                                        }
                                    }

                                    if (memoryChange) {
                                        contractMemory.ComputeHash(ContractMemoryHashAlgorithm.Instance);
                                        oldState.ContractMemory = contractMemory;
                                        change = true;
                                    }
                                }

                                if (change) {
                                    if (!newState.SelfDestruct) {
                                        mpt[addr] = oldState;
                                    } else { // 合约销毁
                                        mpt.Remove(addr);
                                        allContracts = allContracts.Remove(addr);
                                    }
                                }
                            }

                            if (isCreate && !mpt.ContainsKey(contractAddress)) {
                                var contractMemory = new MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>>();
                                contractMemory.ComputeHash(ContractMemoryHashAlgorithm.Instance);
                                mpt.TryGetValue(contractAddress, out var state);
                                state.ContractMemory = contractMemory;
                                mpt[contractAddress] = state;
                            }
                        }

                        // exit:
                        bool noOverflow = true;
                        ulong value = 0;
                        noOverflow &= MathTools.CheckMul(gasUsed, tx.GasPrice, out ulong fee);
                        if (executeSuccess) noOverflow &= MathTools.CheckAdd(fee, context.ValueLeft, out value); // gas费用+转账金额 => 生产者
                        noOverflow &= MathTools.CheckSub(fromState.Balance, value, out ulong fromBalance);

                        if (noOverflow) {
                            producerState.Balance += value;
                            fromState.Balance = fromBalance;
                        } else {
                            producerState.Balance += fromState.Balance;
                            fromState.Balance = 0;
                        }

                        mpt[block.ProducerAddress] = producerState;
                        SetFromState(fromState);
                        if (executeSuccess) return;
                        throw new ExecuteTransactionException((uint)TransactionErrorCode.ContractExecuteFail);
                    }
                }

                mpt[block.ProducerAddress] = producerState;
                SetFromState(fromState);
                return;
            } catch { /*Invalid*/ }

        Invalid:
            throw new InvalidTransactionException(tx);
        }


        unsafe private void InitHostInterface(ContractNative.evmc_host_interface* @interface, ContractContext context) {
            var block = context.Block;
            var tx = context.Transaction;
            var tempStates = context.TempStates;
            var mpt = context.MPT;

            #region local method
            ulong GetBalance(in Bytes<Address> addr) {
                if (tempStates.TryGetValue(addr, out var tempState)) {
                    return tempState.Balance;
                }
                ref readonly var state = ref mpt.GetRefValue(addr);
                return state.IsNull() ? 0 : (ulong)state.Balance;
            }

            Bytes<U256> GetStorage(in Bytes<Address> addr, in Bytes<U256> key) {
                if (tempStates.TryGetValue(addr, out var tempState)) {
                    Debug.Assert(tempState.ContractMemory is not null);
                    if (tempState.ContractMemory.TryGetValue(key, out var value)) {
                        return value; // value可能为0
                    }
                }

                {
                    ref readonly var state = ref mpt.GetRefValue(addr);
                    Debug.Assert(!state.IsNull());
                    Debug.Assert(state.ContractMemory is not null);
                    if (state.ContractMemory.TryGetValue(key, out var value)) {
                        return value; // value不可能为0
                    }
                }
                return default;
            }

            ContractNative.evmc_storage_status SetStorage(in Bytes<Address> addr, in Bytes<U256> key, in Bytes<U256> newValue) {
                Bytes<U256> oldValue = default;
                ref readonly UserState state = ref Unsafe.NullRef<UserState>();
                bool tempFound = false;

                if (tempStates.TryGetValue(addr, out var tempState)) {
                    Debug.Assert(tempState.ContractMemory is not null);
                    tempFound = tempState.ContractMemory.TryGetValue(key, out oldValue); // 如果找到，value可能为0
                }

                if (!tempFound) {
                    state = ref mpt.GetRefValue(addr);
                    Debug.Assert(!state.IsNull());
                    Debug.Assert(state.ContractMemory is not null);
                    state.ContractMemory.TryGetValue(key, out oldValue); // 如果找到，value不可能为0
                }

                if (oldValue == newValue) {
                    return ContractNative.evmc_storage_status.EVMC_STORAGE_UNCHANGED;
                } else {
                    if (tempFound) {
                        Debug.Assert(tempState?.ContractMemory is not null);
                        tempState.ContractMemory[key] = newValue;
                        return ContractNative.evmc_storage_status.EVMC_STORAGE_MODIFIED_AGAIN;
                    } else {
                        if (tempState is null) {
                            tempState = TempState.Contract(state.Balance); // state一定不是null
                            tempStates[addr] = tempState;
                        }
                        Debug.Assert(tempState.ContractMemory is not null);
                        tempState.ContractMemory[key] = newValue;

                        if (oldValue == default) {
                            return ContractNative.evmc_storage_status.EVMC_STORAGE_ADDED;
                        } else if (newValue == default) {
                            return ContractNative.evmc_storage_status.EVMC_STORAGE_DELETED;
                        } else {
                            return ContractNative.evmc_storage_status.EVMC_STORAGE_MODIFIED;
                        }
                    }
                }
            }

            TempState GetStateForCall(in Bytes<Address> addr) {
                if (tempStates.TryGetValue(addr, out var tempState)) {
                    return tempState;
                }

                ref readonly var state = ref mpt.GetRefValue(addr);
                TempState result = state.IsNull() ? TempState.User(0) : // 不存在的地址，默认普通地址
                    state.ContractMemory is null ? TempState.User(state.Balance) :
                    TempState.Contract(state.Balance);
                tempStates[addr] = result;
                return result;
            }

            #endregion

            @interface->account_exists = (ContractNative.evmc_host_context* _, in Bytes<Address> addr) => {
                return tempStates.ContainsKey(addr) || mpt.ContainsKey(addr) ? 1 : 0;
            };

            @interface->get_balance = (ContractNative.evmc_host_context* _, in Bytes<Address> addr) => {
                return GetBalance(addr);
            };

            @interface->get_storage = (ContractNative.evmc_host_context* _, in Bytes<Address> addr, in Bytes<U256> key) => {
                return GetStorage(addr, key);
            };

            @interface->set_storage = (ContractNative.evmc_host_context* _, in Bytes<Address> addr, in Bytes<U256> key, in Bytes<U256> newValue) => {
                return SetStorage(addr, key, newValue);
            };

            @interface->get_block_hash = (ContractNative.evmc_host_context* _, long number) => {
                if (number < 1 || number > LastBlock.Height) return default;
                return db.GetBlockHash((uint)number);
            };

            @interface->copy_code = (ContractNative.evmc_host_context* _, in Bytes<Address> addr, nuint code_offset, byte* buffer, nuint buffer_size) => {
                if (allContracts.TryGetValue(addr, out byte[]? bytecode)) {
                    int copySize = Math.Min(bytecode.Length - (int)code_offset, (int)buffer_size);
                    if (copySize <= 0) return 0;
                    bytecode.AsSpan((int)code_offset, copySize).TryCopyTo(new Span<byte>(buffer, copySize));
                    return (nuint)copySize;
                }
                return 0;
            };

            @interface->get_code_hash = (ContractNative.evmc_host_context* _, in Bytes<Address> addr) => {
                if (allContracts.TryGetValue(addr, out byte[]? bytecode)) {
                    return HashTools.MessageHash(bytecode);
                }
                return Bytes<Hash256>.Empty;
            };

            @interface->get_code_size = (ContractNative.evmc_host_context* _, in Bytes<Address> addr) => {
                if (allContracts.TryGetValue(addr, out byte[]? bytecode)) {
                    return (nuint)bytecode.Length;
                }
                return 0;
            };

            @interface->get_tx_context = delegate {
                return new ContractNative.evmc_tx_context {
                    block_coinbase = block.ProducerAddress,
                    block_number = block.Height,
                    block_timestamp = block.Timestamp,
                    tx_gas_price = new Bytes<U256>(tx.GasPrice),
                    tx_origin = tx.From,
                };
            };

            @interface->selfdestruct = (ContractNative.evmc_host_context* _, in Bytes<Address> addr, in Bytes<Address> beneficiary) => {
                var from = GetStateForCall(addr);
                var to = GetStateForCall(beneficiary);
                to.Balance += from.Balance;
                from.Balance = 0;
                from.SelfDestruct = true;
            };

            @interface->calc_hash = (byte* data, nuint data_size) => {
                return Sha256.ComputeHash(new ReadOnlySpan<byte>(data, (int)data_size));
            };

            @interface->call = (ContractNative.evmc_host_context* _, ContractNative.evmc_message* msg) => {
                if (!msg->Value.TryGetUInt64(out ulong value) || context.ValueLeft < value) goto Fail;

                if (value != 0 && msg->InputSize == 0) { // 普通转账
                    var from = GetStateForCall(msg->Sender);
                    var to = GetStateForCall(msg->Destination);

                    if (from.Balance < value) goto Fail;

                    context.ValueLeft -= value;
                    from.Balance -= value;
                    to.Balance += value;

                    return new ContractNative.evmc_result { StatusCode = ContractNative.evmc_status_code.EVMC_SUCCESS, GasLeft = (ulong)msg->Gas };
                }

                if (allContracts.TryGetValue(msg->Destination, out byte[]? bytecode) && msg->InputSize != 0) {
                    fixed (byte* pBytecode = bytecode) {
                        return ContractNative.VM->Execute(ContractNative.VM, @interface, null, ContractNative.EVMC_MAX_REVISION, msg, pBytecode, (nuint)bytecode.Length);
                    }
                }

            Fail:
                return new ContractNative.evmc_result { StatusCode = ContractNative.evmc_status_code.EVMC_FAILURE, GasLeft = (ulong)msg->Gas };
            };

            @interface->emit_log = delegate { };
        }


        /// <summary>
        /// 是否拥有生产区块的权限
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool IsProducer(Bytes<Address> address) {
            var state = LastBlock.CommitState!;
            int rank = Array.BinarySearch(state.SortedCampaignNodes, address, state.CampaignComparer);
            return 0 <= rank && rank < Constants.MinProducerCount;
        }

        /// <summary>
        /// 是否竞选节点
        /// </summary>
        /// <param name="block"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static bool IsCampaignNode(Block block, in UserState status) {
            if (status.SuperPledge < Constants.MinSuperPledgeCoin) return false;
            if (block.Timestamp - status.SuperPledgeTimestamp >= Constants.YearSeconds) return false;
            return true;
        }

        public PublicKey? GetPublicKey(Bytes<Address> address) => db.GetConfirmedUserPublicKey(address);


        /// <summary>
        /// 提交区块，更新系统内存状态，并更新数据库（如果需要）
        /// </summary>
        /// <param name="block"></param>
        internal void CommitBlock(Block block, bool updateDatabase = true) {
            Debug.Assert(block.PrecommitState is { });
            Debug.Assert(block.CommitState is null);

            block.CommitState = block.PrecommitState;
            block.PrecommitState = null;

            if (block.Height != 1 && LastBlock is not null && LastBlock.Height + 1 == block.Height) {
                if (LastBlock.BigRound.Rounds != block.BigRound.Rounds) {
                    var newCampaignNodes = new ConcurrentDictionary<Bytes<Address>, SuperPeer?>();
                    foreach (var addr in block.CommitState.SortedCampaignNodes) newCampaignNodes.TryAdd(addr, null);

                    foreach (var (oldAddress, oldNode) in CampaignNodes) {
                        if (newCampaignNodes.ContainsKey(oldAddress)) {
                            newCampaignNodes[oldAddress] = oldNode;
                        } else {
                            oldNode?.Dispose();
                        }
                    }

                    CampaignNodes = newCampaignNodes; // 更新竞选节点信息
                }
            } else if (block.Height == 1 && LastBlock is null) { // 创世区块
                var campaignNodes = new ConcurrentDictionary<Bytes<Address>, SuperPeer?>();
                foreach (var addr in block.CommitState.SortedCampaignNodes) {
                    campaignNodes.TryAdd(addr, null);
                }
                CampaignNodes = campaignNodes;

                if (updateDatabase) {
                    var users = new Dictionary<Bytes<Address>, PublicKey?>();
                    foreach (var (address, (publicKey, _)) in Config.GenesisUsers) {
                        users.Add(address, publicKey);
                    }
                    db.PutUsers(users);
                }
            } else {
                throw new InvalidBlockException();
            }

            if (updateDatabase) db.PutBlock(block);
            cacheBlocks.Enqueue(block, out _);

            LastBlock = block;
        }

        /// <summary>
        /// 尝试统计票数并排名
        /// </summary>
        /// <param name="block"></param>
        internal void TryStatisticsCampaignNodes(Block block) {
            if (LastBlock.BigRound.Rounds == block.BigRound.Rounds) return;

            Debug.Assert(block.PrecommitState is { });

            BlockState result = StatisticsCampaignNodes(block.PrecommitState);
            block.PrecommitState.TempCampaignNodes = result.TempCampaignNodes;
            block.PrecommitState.CampaignNodeRecord = result.CampaignNodeRecord;
        }

        internal static BlockState StatisticsCampaignNodes(BlockState currentState) {
            var campaignComparer = new CampaignComparer(currentState.WorldState);
            Bytes<Address>[] allCampaignNodes = currentState.SortedCampaignNodes.Concat(currentState.TempCampaignNodes).Distinct().ToArray();
            Array.Sort(allCampaignNodes, campaignComparer);

            int removeCount = 0;
            for (int i = 0; i < allCampaignNodes.Length; i++) {
                ref readonly Bytes<Address> addr = ref allCampaignNodes[^(i + 1)];
                ref readonly UserState status = ref currentState.WorldState.GetRefValue(addr);
                if (status.IsNull() || (ulong)status.SuperPledge is 0) {
                    removeCount++;
                } else {
                    break;
                }
            }

            if (removeCount != 0) {
                allCampaignNodes = allCampaignNodes[..^removeCount];
            }

            var record = new List<SortedCampaignNodeArray>(currentState.CampaignNodeRecord) {
                new SortedCampaignNodeArray(allCampaignNodes, campaignComparer)
            };
            if (record.Count > 3) {
                record.RemoveAt(0);
            }

            return new BlockState {
                CampaignNodeRecord = record.ToArray(),
                TempCampaignNodes = new List<Bytes<Address>>(),
            };
        }

        public static Block BuildGenesisBlock() {
            var state = new BlockState {
                WorldState = new MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>>(1),
                Transactions = new MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>>(),
                TempCampaignNodes = new List<Bytes<Address>>(),
            };

            var campaignNodes = new List<Bytes<Address>>();
            foreach (var (address, (_, userState)) in Config.GenesisUsers) {
                state.WorldState.Add(address, userState);

                if ((ulong)userState.SuperPledge != 0) {
                    campaignNodes.Add(address);
                }
            }

            state.Transactions.ComputeHash(TransactionHashAlgorithm.Instance);
            state.WorldState.ComputeHash(UserStateHashAlgorithm.Instance);

            var campaignComparer = new CampaignComparer(state.WorldState);
            campaignNodes.Sort(campaignComparer);

            state.CampaignNodeRecord = new[] { new SortedCampaignNodeArray(campaignNodes.ToArray(), campaignComparer) };

            var result = new Block {
                Version = Constants.BlockVersion,
                Height = 1,
                Timestamp = 0,
                HashPrevBlock = Bytes<Hash256>.Empty,
                HashWorldState = state.WorldState.RootHash,
                HashTxMerkleRoot = state.Transactions.RootHash,
                ProducerPublicKey = Config.MyPublicKey,
                Signature = new Signature(Secp256k1.Math.U256.Zero, Secp256k1.Math.U256.Zero),

                ProducerAddress = Bytes<Address>.Empty,
                Transactions = Array.Empty<Transaction>(),
                PrecommitState = state
            };

            result.ComputeHash();

            return result;
        }


        private bool disposedValue = false;

        void Dispose(bool disposing) {
            if (!disposedValue) {
                blockChannel.Writer.TryComplete();
                if (disposing) {
                    db.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);


        sealed class CampaignComparer : IComparer<Bytes<Address>> {
            private readonly MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt;

            public CampaignComparer(MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt) => this.mpt = mpt;

            unsafe public int Compare(Bytes<Address> x, Bytes<Address> y) {
                if (x == y) return 0;

                ref readonly UserState xValue = ref mpt.GetRefValue(x);
                ref readonly UserState yValue = ref mpt.GetRefValue(y);
                if (xValue.IsNull() & yValue.IsNull()) return x.CompareTo(y);
                if (xValue.IsNull()) return 1;
                if (yValue.IsNull()) return -1;
                ulong xVotes = xValue.SuperPledge + xValue.Votes * 4;
                ulong yVotes = yValue.SuperPledge + yValue.Votes * 4;
                if (xVotes == yVotes) {
                    if (xValue.SuperPledgeTimestamp != yValue.SuperPledgeTimestamp)
                        return xValue.SuperPledgeTimestamp.CompareTo(yValue.SuperPledgeTimestamp);
                    return x.CompareTo(y);
                }
                return yVotes.CompareTo(xVotes);
            }
        }

        unsafe internal sealed class UserStateHashAlgorithm : MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>>.IHashAlgorithm Instance = new UserStateHashAlgorithm();

            private UserStateHashAlgorithm() { }

            public Bytes<Hash256> ComputeHash(Bytes<Address> key, in UserState value) {
                var serializer = new Serializer(capacity: 4096);
                serializer.WriteU64LittleEndian(value.Balance);
                serializer.WriteU64LittleEndian(value.NextNonce);
                serializer.WriteU64LittleEndian(value.VotePledge);
                serializer.WriteAddresses(value.VoteAddresses);
                serializer.WriteU64LittleEndian(value.SuperPledge);
                serializer.WriteU64LittleEndian(value.VotePledgeTimestamp);
                serializer.WriteU64LittleEndian(value.SuperPledgeTimestamp);
                serializer.WriteU64LittleEndian(value.Votes);
                var locks = value.Locks ?? Array.Empty<LockStatus>();
                serializer.WriteU64LittleEndian((ulong)locks.Length);
                foreach (var @lock in locks) {
                    serializer.WriteU64LittleEndian(@lock.Value);
                    serializer.WriteU64LittleEndian(@lock.UnlockTimestamp);
                }
                serializer.Write(value.ContractMemory?.RootHash ?? default);
                return serializer.RawData.MessageHash();
            }

            public Bytes<Hash256> ComputeHash(ReadOnlySpan<Bytes<Hash256>> hashes) {
                return hashes.HashesHash();
            }
        }

        unsafe internal sealed class TransactionHashAlgorithm : MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>>.IHashAlgorithm Instance = new TransactionHashAlgorithm();

            private TransactionHashAlgorithm() { }

            public Bytes<Hash256> ComputeHash(Bytes<Hash256> key, in TransactionResult tx) {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, tx.ErrorCode ?? 0);
                return HashTools.HashesHash(stackalloc[] { key, buffer.MessageHash() });
            }

            public Bytes<Hash256> ComputeHash(ReadOnlySpan<Bytes<Hash256>> hashes) {
                return hashes.HashesHash();
            }
        }

        unsafe internal sealed class ContractMemoryHashAlgorithm : MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>>.IHashAlgorithm Instance = new ContractMemoryHashAlgorithm();

            private ContractMemoryHashAlgorithm() { }

            public Bytes<Hash256> ComputeHash(Bytes<U256> key, in Bytes<U256> value) {
                Span<byte> buffer = stackalloc byte[sizeof(U256) * 2];
                key.CopyTo(buffer[..sizeof(U256)]);
                value.CopyTo(buffer[sizeof(U256)..]);
                return buffer.MessageHash();
            }

            public Bytes<Hash256> ComputeHash(ReadOnlySpan<Bytes<Hash256>> hashes) {
                return hashes.HashesHash();
            }
        }

        private sealed class TempState {
            public ulong Balance;
            public Dictionary<Bytes<U256>, Bytes<U256>>? ContractMemory;
            public bool SelfDestruct;

            public static TempState Contract(ulong balance) {
                return new TempState {
                    Balance = balance,
                    ContractMemory = new Dictionary<Bytes<U256>, Bytes<U256>>()
                };
            }

            public static TempState User(ulong balance) {
                return new TempState {
                    Balance = balance
                };
            }
        }

        private sealed class ContractContext {
            public readonly Block Block;
            public readonly Transaction Transaction;
            public readonly Dictionary<Bytes<Address>, TempState> TempStates;
            public readonly MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> MPT;

            public ulong ValueLeft;

            public ContractContext(Block block, Transaction transaction, Dictionary<Bytes<Address>, TempState> tempStates, MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> mpt) {
                Block = block;
                Transaction = transaction;
                TempStates = tempStates;
                MPT = mpt;
            }
        }
    }
}
