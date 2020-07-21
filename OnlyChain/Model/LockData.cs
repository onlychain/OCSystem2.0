using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Model {
    public sealed class LockData : AttachData {
        /// <summary>
        /// 'LOCK'
        /// </summary>
        public const uint Prefix = 0x4c4f434b;

        public uint UnlockTimestamp { get; }

        public LockData(uint unlockTimestamp) => UnlockTimestamp = unlockTimestamp;
    }
}
