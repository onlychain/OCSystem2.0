using System;
using System.Collections.Generic;
using System.Text;

namespace OnlyChain.Secp256k1 {
	public class InvalidPrivateKeyException : Exception {
		public InvalidPrivateKeyException() : base("无效的私钥") { }
		public InvalidPrivateKeyException(string message) : base(message) { }
	}
}
