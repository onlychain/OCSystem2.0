using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public enum TransactionErrorCode : uint {
        NoError,
        InsufficientBalance,
        PledgeTooLow,
        ValueNotEqualToZero,
        Unpledged,
        NotExpired,
        VoteTooLow,
        DuplicateAddress,

    }
}
