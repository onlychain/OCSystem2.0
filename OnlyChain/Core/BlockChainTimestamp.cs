using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public static class BlockChainTimestamp {
        // TODO: 创世时间
        //public static readonly DateTime GenesisDateTime = DateTime.UtcNow;
        public static readonly DateTime GenesisDateTime = new DateTime(2020, 11, 28, 16, 33, 0, DateTimeKind.Local).ToUniversalTime();

        public static uint ToTimestamp(DateTime dateTime) {
            TimeSpan timeSpan = dateTime - GenesisDateTime;
            return (uint)Math.Round(timeSpan.TotalSeconds);
        }

        public static DateTime ToDateTime(uint timestamp) {
            return GenesisDateTime + TimeSpan.FromSeconds(timestamp);
        }

        public static uint NowTimestamp => ToTimestamp(DateTime.UtcNow);
    }
}
