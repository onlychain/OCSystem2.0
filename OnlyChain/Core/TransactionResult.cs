using OnlyChain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public readonly struct TransactionResult {
        public readonly Transaction Transaction;
        public readonly uint? ErrorCode;

        public TransactionResult(Transaction transaction, uint? errorCode = null) {
            Transaction = transaction;
            ErrorCode = errorCode;
        }
    }
}
