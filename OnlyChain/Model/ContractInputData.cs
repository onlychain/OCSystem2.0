using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Model {
    public sealed class ContractInputData : AttachData {
        public readonly ReadOnlyMemory<byte> Input;

        public ContractInputData(ReadOnlyMemory<byte> input) => Input = input;
    }
}
