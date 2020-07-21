#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Core {
    public sealed class ExecuteTransactionException : Exception {
        /// <summary>
        /// 错误报告。当此值为null时，表示数据太大而被忽略。
        /// </summary>
        public readonly uint ErrorCode;

        public ExecuteTransactionException(uint errorCode) : base("交易执行错误") {
            ErrorCode = errorCode;
        }
    }
}
