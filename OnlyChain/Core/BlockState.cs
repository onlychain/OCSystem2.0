#nullable disable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class BlockState {
        /// <summary>
        /// 世界MPT
        /// </summary>
        public MerklePatriciaTree<Address, UserState, Hash<Size256>> WorldState { get; set; }
        /// <summary>
        /// 交易MPT
        /// </summary>
        public MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>> Transactions { get; set; }
        /// <summary>
        /// 本轮新增的竞选节点
        /// </summary>
        public ICollection<Address> TempCampaignNodes { get; set; }
        /// <summary>
        /// 上一轮已统计的竞选节点
        /// </summary>
        public Address[] SortedCampaignNodes { get; set; }
        /// <summary>
        /// 竞选节点比较器
        /// </summary>
        public IComparer<Address> CampaignComparer { get; set; }

        public BlockState NextNew() {
            return new BlockState() {
                WorldState = WorldState.NextNew(),
                Transactions = new MerklePatriciaTree<Hash<Size256>, TransactionResult, Hash<Size256>>(0),
                TempCampaignNodes = TempCampaignNodes,
                SortedCampaignNodes = SortedCampaignNodes,
                CampaignComparer = CampaignComparer,
            };
        }
    }
}
