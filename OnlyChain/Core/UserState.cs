#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public struct UserState {
        /// <summary>
        /// 账户余额
        /// </summary>
        public Coin Balance;
        /// <summary>
        /// 下一个交易索引（交易中的Nonce字段必须与此字段一致才是合法交易）
        /// </summary>
        public ulong NextNonce;
        /// <summary>
        /// 投票质押代币数
        /// </summary>
        public Coin VotePledge;
        /// <summary>
        /// 投票给的地址
        /// </summary>
        public Bytes<Address>[]? VoteAddresses;
        /// <summary>
        /// 超级节点竞选质押代币数
        /// </summary>
        public Coin SuperPledge;
        /// <summary>
        /// 投票质押时间戳
        /// </summary>
        public uint VotePledgeTimestamp;
        /// <summary>
        /// 超级节点竞选质押时间戳
        /// </summary>
        public uint SuperPledgeTimestamp;
        /// <summary>
        /// 得票权重
        /// </summary>
        public ulong Votes;
        /// <summary>
        /// 锁仓列表（按解锁时间戳从小到大排序）
        /// </summary>
        public LockStatus[]? Locks;
        /// <summary>
        /// 合约内存（不包含字节码和元数据）
        /// </summary>
        public MerklePatriciaTree<Bytes<U256>, Bytes<U256>, Bytes<Hash256>>? ContractMemory;
    }
}
