using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public enum ProduceState {
        /// <summary>
        /// 等待生产者的区块碎片
        /// </summary>
        WaitProducerBlockChip,
        /// <summary>
        /// 等待其他人的区块碎片
        /// </summary>
        WaitOtherBlockChip,

        
    }
}
