#nullable enable

using OnlyChain.Database;
using OnlyChain.Model;
using OnlyChain.Network;
using OnlyChain.Secp256k1;
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public sealed class BlockChainSystem : IDisposable {
        const int CacheHeight = 150; // 最大缓存数量

        static readonly Regex wordRegex = new Regex(@"^\w+$", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly IClient client;

        private readonly LevelDB immutableDatabase; 
        private readonly UserDictionary userDatabse; // 用户字典
        private readonly Hashes<Hash<Size256>> blockHashes; // 所有区块的hash以及本地索引
        private readonly IndexedQueue<Block> cacheBlocks = new IndexedQueue<Block>(CacheHeight); // 缓存最近的区块

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly Channel<Block> blockChannel = Channel.CreateUnbounded<Block>(new UnboundedChannelOptions { SingleReader = true });

        public string ChainName { get; }

        public Block LastBlock { get; private set; } = null!;
        public Dictionary<Address, SuperNode?> CampaignNodes { get; private set; } = new Dictionary<Address, SuperNode?>(); // 所有竞选节点与IP端口(TCP)

        public event EventHandler<CampaignNodesChangedEventArgs>? CampaignNodesChanged;
        public event EventHandler<SystemStateTransferredEventArgs>? StateTransferred;



        public BlockChainSystem(IClient client, string chainName = "main") {
            this.client = client;

            ChainName = chainName ?? throw new ArgumentNullException(nameof(chainName));
            if (!wordRegex.IsMatch(chainName)) throw new ArgumentOutOfRangeException(nameof(chainName), "非法的链名");

            if (!Directory.Exists(chainName)) Directory.CreateDirectory(chainName);

            immutableDatabase = new LevelDB(Path.Combine(chainName, "block_chain"), new LevelDBOptions {
                CreateIfMissing = true,
                Cache = new LevelDBCache(1000),
            });
            userDatabse = new UserDictionary(Path.Combine(chainName, "block_chain.users"));
            blockHashes = new Hashes<Hash<Size256>>(Path.Combine(chainName, "block_chain.hash"));

            // TODO: 更新创世区块状态

            CommitBlock(Block.GenesisBlock);

            _ = Task.Run(LoopReadBlock);
        }

        public PublicKey? GetPublicKey(Address address) => userDatabse.TryGetValue(address, out var value) ? value : null;

        private async void LoopReadBlock() {
            await foreach (var block in blockChannel.Reader.ReadAllAsync(cancellationTokenSource.Token)) {
                Put(block);
            }
        }

        /// <summary>
        /// 添加一个新的区块到区块链
        /// </summary>
        /// <param name="block"></param>
        private void Put(Block block) {
            try {
                // 添加一个新的高度的区块
                VerifyExecuteBlock(block);
                cacheBlocks.Enqueue(block, out _);
                StateTransition(block);

                int nativeBlockIndex = blockHashes.Add(block.Hash);
                // TODO: 把区块写入数据库

            } catch {

            }
        }

        private void ExecuteGenesisBlock(Block block) {
            block.PrecommitState = new BlockState {
                WorldState = new MerklePatriciaTree<Address, UserState, Hash<Size256>>(1),
                Transactions = new MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>>(0),

            };
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

            var verifyTasks = new List<Task>();
            var verifyCancellationTokenSource = new CancellationTokenSource();

            // 验证生产者
            if (!IsProducer(block.ProducerAddress)) throw new InvalidBlockException();

            // 验证签名，验证交易mpt
            verifyTasks.Add(Task.Run(delegate {
                bool verifyResult = Ecdsa.Verify(GetProducerPublicKey(block), block.HashSignHeader, block.Signature);
                if (verifyResult is false) throw new InvalidBlockException();
            }));

            // TODO: 验证出块时间


            // 并行验证交易签名
            int taskCount = Math.Max(Environment.ProcessorCount - 2, 1);
            var perCount = (int)Math.Ceiling(block.Transactions.Length / (double)taskCount);
            int counter = 0;
            while (counter < block.Transactions.Length) {
                int processCount = Math.Min(perCount, block.Transactions.Length - counter);
                int startIndex = counter;
                verifyTasks.Add(Task.Run(delegate {
                    for (int i = 0; i < processCount && !verifyCancellationTokenSource.IsCancellationRequested; i++) {
                        Transaction transaction = block.Transactions[i + startIndex];
                        Hash<Size256> hash = transaction.HashSignHeader;
                        bool verifyResult = Ecdsa.Verify(transaction.PublicKey, hash, transaction.Signature);
                        if (verifyResult is false) throw new InvalidBlockException();
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
            } catch {
                verifyCancellationTokenSource.Cancel();
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
        internal void ExecuteTransaction(Block block, Transaction tx) {
            try {
                Debug.Assert(block.PrecommitState is { });

                if ((ulong)tx.GasPrice is 0) goto Invalid; // 手续费不能为0
                if (tx.GasLimit < tx.BaseGasUsed) goto Invalid; // GasLimit比最低汽油费还低

                var mpt = block.PrecommitState.WorldState;
                UserState fromState = mpt[tx.From];
                if (fromState.NextNonce != tx.Nonce) goto Invalid; // nonce不正确
                ulong minFee = checked(tx.BaseGasUsed * tx.GasPrice); // 最低手续费

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

                if (fromState.Balance < minFee) goto Invalid; // 不足以支付最低手续费

                UserState producerState = mpt[block.ProducerAddress];
                fromState.Balance -= minFee; // 扣除最低手续费
                producerState.Balance += minFee;

                UserState rollbackFromStatus = fromState; // 保存当前状态，交易执行失败时回滚
                void Fail(TransactionErrorCode errorCode) {
                    mpt[block.ProducerAddress] = producerState;
                    SetFromState(rollbackFromStatus);
                    throw new ExecuteTransactionException((uint)errorCode);
                }

                void SetFromState(in UserState state) {
                    bool remove = (ulong)state.Balance is 0
                        && (ulong)state.VotePledge is 0
                        && (ulong)state.SuperPledge is 0
                        && state.Votes is 0;
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
                    case SuperPledgeData _: // 超级节点质押，本轮结束其他账户才能对自己投票
                        if ((ulong)fromState.SuperPledge is 0 && tx.Value < Constants.MinSuperPledgeCoin) Fail(TransactionErrorCode.PledgeTooLow); // 质押金额太小
                        if (fromState.SuperPledge >= Constants.MinSuperPledgeCoin && tx.Value < Constants.MinSuperPledgeIncrement) Fail(TransactionErrorCode.PledgeTooLow); // 质押金额太小
                        fromState.SuperPledge += tx.Value;
                        fromState.SuperPledgeTimestamp = block.Timestamp;
                        block.PrecommitState.TempCampaignNodes.Add(tx.From);
                        break;
                    case SuperRedemptionData _: // 超级节点赎回
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
                        if (vote.Addresses.Length != new HashSet<Address>(vote.Addresses).Count) Fail(TransactionErrorCode.DuplicateAddress); // 地址重复

                        var changedStatus = new Dictionary<Address, UserState>(60);

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

                        foreach (Address addr in vote.Addresses) {
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
                    case VoteRedemptionData _: // 投票质押赎回
                        if ((ulong)tx.Value != 0) Fail(TransactionErrorCode.ValueNotEqualToZero); // Value不为0
                        if (fromState.VoteAddresses is null) Fail(TransactionErrorCode.Unpledged); // 从未质押过
                        if (block.Timestamp - fromState.VotePledgeTimestamp < Constants.VotePledgeSeconds) Fail(TransactionErrorCode.NotExpired);

                        for (int i = 0; i < fromState.VoteAddresses!.Length; i++) {
                            Address addr = fromState.VoteAddresses[i];
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
                }

                mpt[block.ProducerAddress] = producerState;
                SetFromState(fromState);
                return;
            } catch { /*Invalid*/ }
        Invalid:
            throw new InvalidTransactionException(tx);
        }


        /// <summary>
        /// 是否拥有生产区块的权限
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool IsProducer(Address address) {
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

        private PublicKey GetProducerPublicKey(Block block) {
            return userDatabse[block.ProducerAddress];
        }

        internal void CommitBlock(Block block) {
            Debug.Assert(block.PrecommitState is { });
            Debug.Assert(block.CommitState is null);

            block.CommitState = block.PrecommitState;
            block.PrecommitState = null;

            foreach (var tx in block.Transactions) {
                userDatabse.Set(tx.From, tx.PublicKey);
            }

            if (LastBlock.BigRound.Rounds != block.BigRound.Rounds) {
                var newCampaignNodes = new Dictionary<Address, SuperNode?>();
                foreach (var addr in block.CommitState.SortedCampaignNodes) newCampaignNodes.Add(addr, null);

                foreach (var (oldAddress, oldNode) in CampaignNodes) {
                    if (newCampaignNodes.ContainsKey(oldAddress)) {
                        newCampaignNodes[oldAddress] = oldNode;
                    } else {
                        oldNode?.Dispose();
                    }
                }

                CampaignNodes = newCampaignNodes; // 更新竞选节点信息
            }

            LastBlock = block;
        }

        /// <summary>
        /// 让系统状态在相邻的区块间转移
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void StateTransition(Block to) {
            Block from = LastBlock;
            LastBlock = to;

            var eventArgs = new SystemStateTransferredEventArgs(from, to);
            StateTransferred?.Invoke(this, eventArgs);
        }

        private void Forward(Block next) {

        }

        private void Rollback(Block prev) {

        }

        unsafe private Block? ReadBlockFromDatabase(Hash<Size256> key) {
            byte[]? blockByteData = immutableDatabase.Get(new ReadOnlySpan<byte>(&key, sizeof(Hash<Size256>)));
            if (blockByteData is null) return null;
            return new Block(blockByteData, hasTx: true);
        }

        /// <summary>
        /// 尝试统计票数并排名
        /// </summary>
        /// <param name="block"></param>
        internal void TryStatisticsCampaignNodes(Block block) {
            if (LastBlock.BigRound.Rounds == block.BigRound.Rounds) return;
            
            Debug.Assert(block.PrecommitState is { });

            BlockState result = StatisticsCampaignNodes(block.PrecommitState);
            block.PrecommitState.CampaignComparer = result.CampaignComparer;
            block.PrecommitState.SortedCampaignNodes = result.SortedCampaignNodes;
            block.PrecommitState.TempCampaignNodes = result.TempCampaignNodes;
        }

        internal static BlockState StatisticsCampaignNodes(BlockState currentState) {
            var campaignComparer = new CampaignComparer(currentState.WorldState);
            Address[] allCampaignNodes = currentState.SortedCampaignNodes.Concat(currentState.TempCampaignNodes).Distinct().ToArray();
            Array.Sort(allCampaignNodes, campaignComparer);

            int removeCount = 0;
            for (int i = 0; i < allCampaignNodes.Length; i++) {
                ref readonly Address addr = ref allCampaignNodes[^(i + 1)];
                ref readonly UserState status = ref currentState.WorldState.TryGetValue(addr);
                if (status.IsNull() || (ulong)status.SuperPledge is 0) {
                    removeCount++;
                } else {
                    break;
                }
            }

            var result = new BlockState {
                WorldState = null!,
                Transactions = null!,
                CampaignComparer = campaignComparer,
            };
            if (removeCount != 0) {
                result.SortedCampaignNodes = allCampaignNodes[..^removeCount];
            } else {
                result.SortedCampaignNodes = allCampaignNodes;
            }
            result.TempCampaignNodes = new List<Address>();
            return result;
        }

        private bool disposedValue = false;

        void Dispose(bool disposing) {
            if (!disposedValue) {
                blockChannel.Writer.TryComplete();
                if (disposing) {
                    immutableDatabase.Dispose();
                    blockHashes.Dispose();
                    userDatabse.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        unsafe public bool ContainsKey(Hash<Size256> key)
            => blockHashes.ContainsKey(key);

        public bool TryGetValue(Hash<Size256> key, [MaybeNullWhen(false)] out Block value) {
            value = ReadBlockFromDatabase(key);
            return value is { };
        }


        sealed class CampaignComparer : IComparer<Address> {
            private readonly MerklePatriciaTree<Address, UserState, Hash<Size256>> mpt;

            public CampaignComparer(MerklePatriciaTree<Address, UserState, Hash<Size256>> mpt) => this.mpt = mpt;

            unsafe public int Compare(Address x, Address y) {
                if (x == y) return 0;
                ref readonly UserState xValue = ref mpt.TryGetValue(x);
                ref readonly UserState yValue = ref mpt.TryGetValue(y);
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

        unsafe internal sealed class UserStateHashAlgorithm : MerklePatriciaTree<Address, UserState, Hash<Size256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Address, UserState, Hash<Size256>>.IHashAlgorithm Instance = new UserStateHashAlgorithm();

            private UserStateHashAlgorithm() { }

            public Hash<Size256> ComputeHash(Address key, in UserState value) {
                Span<byte> buffer = stackalloc byte[1024];
                var serializer = new Serializer(buffer);
                serializer.Write(Serializer.U64LittleEndian, value.Balance);
                serializer.Write(Serializer.U64LittleEndian, value.NextNonce);
                serializer.Write(Serializer.U64LittleEndian, value.VotePledge);
                serializer.Write(Serializer.Addresses, value.VoteAddresses);
                serializer.Write(Serializer.U64LittleEndian, value.SuperPledge);
                serializer.Write(Serializer.U64LittleEndian, value.VotePledgeTimestamp);
                serializer.Write(Serializer.U64LittleEndian, value.SuperPledgeTimestamp);
                serializer.Write(Serializer.U64LittleEndian, value.Votes);
                return buffer[..serializer.Index].MessageHash();
            }

            public Hash<Size256> ComputeHash(ReadOnlySpan<Hash<Size256>> hashes) {
                return hashes.HashesHash();
            }
        }

        unsafe internal sealed class TransactionHashAlgorithm : MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>>.IHashAlgorithm {
            public static readonly MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>>.IHashAlgorithm Instance = new TransactionHashAlgorithm();

            private TransactionHashAlgorithm() { }

            public Hash<Size256> ComputeHash(Hash<Size256> key, in TransactionResult tx) {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, tx.ErrorCode ?? 0);
                return HashTools.HashesHash(stackalloc[] { key, HashTools.MessageHash(buffer) });
            }

            public Hash<Size256> ComputeHash(ReadOnlySpan<Hash<Size256>> hashes) {
                return hashes.HashesHash();
            }
        }
    }
}
