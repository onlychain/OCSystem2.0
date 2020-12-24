using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    public sealed class BString : BValue<string> {
        public const byte PrefixChar = (byte)'s';

        internal static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        public BString(string value) : base(value) { }

        public static implicit operator BString(string value) => new BString(value);

        protected override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            WriteNoPrefix(stream, Value);
        }

        internal static void WriteNoPrefix(Stream stream, string value) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            stream.WriteVarUInt((ulong)bytes.Length);
            stream.Write(bytes);
        }


        public override string ToString() => Value;
    }
}
