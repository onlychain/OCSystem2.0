using System;
using System.IO;
using System.Text;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    public sealed class BString : BValue<string> {
        public const byte PrefixChar = (byte)'s';

        internal static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        public BString(string value) : base(value) { }

        public static implicit operator BString(string value) => new BString(value);

        public override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            WriteNoPrefix(stream, Value);
        }

        internal static void WriteNoPrefix(Stream stream, string value) {
            stream.Write(Encoding.UTF8.GetBytes(value));
            stream.WriteByte(0);
        }

        public override string ToString() => Value;
    }
}
