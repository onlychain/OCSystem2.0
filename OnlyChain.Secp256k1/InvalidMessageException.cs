using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Secp256k1 {
	public class InvalidMessageException : Exception {
		public InvalidMessageException() : base("无效的消息") { }
		public InvalidMessageException(string message) : base(message) { }
	}
}
