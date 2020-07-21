using OnlyChain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class InvalidTransactionException : Exception {
        public Transaction Transaction { get; }

        public InvalidTransactionException(Transaction transaction) : base($"无效的交易: {transaction}") {
            Transaction = transaction;
        }

    }
}
