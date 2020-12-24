#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class ExecuteTransactionException : Exception {
        /// <summary>
        /// 错误代码。
        /// </summary>
        public readonly uint ErrorCode;

        public ExecuteTransactionException(uint errorCode) : base("交易执行错误") {
            ErrorCode = errorCode;
        }
    }
}
