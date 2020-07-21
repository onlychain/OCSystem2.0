using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Network {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandHandlerAttribute : Attribute {
        public string Command { get; }

        public CommandHandlerAttribute(string command) {
            if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException("命令不能为空", nameof(command));
            Command = command;
        }
    }
}
