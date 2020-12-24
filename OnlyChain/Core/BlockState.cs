#nullable disable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class BlockState {
        /// <summary>
        /// 世界MPT
        /// </summary>
        public MerklePatriciaTree<Bytes<Address>, UserState, Bytes<Hash256>> WorldState { get; set; }
        /// <summary>
        /// 交易MPT
        /// </summary>
        public MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>> Transactions { get; set; }
        /// <summary>
        /// 本轮新增的竞选节点
        /// </summary>
        public ICollection<Bytes<Address>> TempCampaignNodes { get; set; }
        /// <summary>
        /// 当前统计的竞选节点
        /// </summary>
        public Bytes<Address>[] SortedCampaignNodes => CampaignNodeRecord[0].Nodes;
        /// <summary>
        /// 
        /// </summary>
        public IComparer<Bytes<Address>> CampaignComparer => CampaignNodeRecord[0].Comparer;
        /// <summary>
        /// [上上一轮统计][上一轮统计][本轮统计]
        /// </summary>
        public SortedCampaignNodeArray[] CampaignNodeRecord { get; set; }

        public void PutCampaignNodes(Bytes<Address>[] sortedCampaignNodes, IComparer<Bytes<Address>> comparer) {
            const int MaxCount = 3;

            if (CampaignNodeRecord is null or { Length: 0 }) {
                CampaignNodeRecord = new SortedCampaignNodeArray[1] ;
            } else if (CampaignNodeRecord.Length < MaxCount) {
                var newCampaignNodeRecord = new SortedCampaignNodeArray[CampaignNodeRecord.Length + 1];
                CampaignNodeRecord.CopyTo(newCampaignNodeRecord, 0);
                CampaignNodeRecord = newCampaignNodeRecord;
            } else {
                CampaignNodeRecord = CampaignNodeRecord[..];
                for (int i = 0; i < MaxCount - 1; i++) {
                    CampaignNodeRecord[i] = CampaignNodeRecord[i + 1];
                }
            }
            CampaignNodeRecord[^1] = new SortedCampaignNodeArray(sortedCampaignNodes, comparer);
        }

        public BlockState NextNew() {
            return new BlockState() {
                WorldState = WorldState.NextNew(),
                Transactions = new MerklePatriciaTree<Bytes<Hash256>, TransactionResult, Bytes<Hash256>>(0),
                TempCampaignNodes = TempCampaignNodes,
                CampaignNodeRecord = CampaignNodeRecord,
            };
        }
    }
}
