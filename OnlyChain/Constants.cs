using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain {
    public static class Constants {
        public readonly static Coin MinSuperPledgeCoin = Coin.FromWhole(300_0000m);
        public readonly static Coin MinSuperPledgeIncrement = Coin.FromWhole(100_0000m);
        public readonly static Coin MinVotePledgeCoin = Coin.FromWhole(1m);
        public readonly static int MinProducerCount = 21;
        public readonly static int MaxProducerCount = 30;
        public readonly static int MinProducerVoteAccpetCount = 15;
        public readonly static uint BlockVersion = 1;
        public readonly static uint YearSeconds = 31536000; // 一年的秒数
        public readonly static uint VotePledgeSeconds = 259200; // 投票可赎回秒数
        public readonly static int MaxTxErrorMessageBytes = 1024;
        public readonly static int BlacklistBlockCount = 1000;
        public readonly static int BlacklistTxCount = 10000;
        public readonly static uint RoundTimestamp = 126;
        public readonly static int MaxBlockBytes = 1048576;
        public readonly static TimeSpan ProduceTimeOffset = TimeSpan.FromSeconds(1);
        public readonly static int SmallRoundSeconds = 42;
        public readonly static int BigRoundSeconds = 126;
        public readonly static TimeSpan ProduceTimeSpan = TimeSpan.FromSeconds(2);
    }
}
