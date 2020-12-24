#nullable enable

using OnlyChain.Core;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OnlyChain.Model {
    public abstract class AttachData {
        public static AttachData? ParseData(Transaction tx) {
            try {
                if (tx.Data is null or { Length: 0 }) return null;

                var deserializer = new Deserializer(tx.Data);
                byte cmd = deserializer.Read<byte>();

                switch (cmd) {
                    case 0x00: // 自由数据
                        if (tx.Data.Length > 4096 + 1) goto Fail;
                        return null;
                    case 0x01: // 投票
                        if (tx.To != Bytes<Address>.Empty) goto Fail;
                        uint round = deserializer.Read(Deserializer.VarUInt32);
                        int count = Math.DivRem(tx.Data.Length - deserializer.Index, Bytes<Address>.Size, out int r);
                        if (r != 0) goto Fail;
                        return new VoteData(round, deserializer.ReadValues<Bytes<Address>>(count));
                    case 0x02: // 投票赎回
                        if (tx.To != Bytes<Address>.Empty) goto Fail;
                        if (tx.Value != 0) goto Fail;
                        return new VoteRedemptionData();
                    case 0x03: // 超级节点质押
                        if (tx.To != Bytes<Address>.Empty) goto Fail;
                        return new SuperPledgeData();
                    case 0x04: // 超级节点赎回
                        if (tx.To != Bytes<Address>.Empty) goto Fail;
                        if (tx.Value != 0) goto Fail;
                        return new SuperRedemptionData();
                    case 0x05: // 锁仓
                        uint unlockTimestamp = deserializer.Read(Deserializer.VarUInt32);
                        return new LockData(unlockTimestamp);
                    case 0xff: // 调用合约
                        return new ContractInputData(tx.Data.AsMemory(1));
                }
            } catch {

            }

        Fail:
            throw new FormatException();
        }
    }
}
