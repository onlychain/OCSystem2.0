using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public enum ClientState {
        /// <summary>
        /// 未初始化状态
        /// </summary>
        Uninitialized,
        /// <summary>
        /// 就绪
        /// </summary>
        Ready,
        /// <summary>
        /// 初始化中
        /// </summary>
        Initializing,
        /// <summary>
        /// 同步中
        /// </summary>
        Synchronizing,
        /// <summary>
        /// 暂停生产，与其他超级节点同步中
        /// </summary>
        ProduceSynchronizing,
    }
}
