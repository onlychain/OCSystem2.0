using System;
using System.IO;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    public sealed class BInt : BValue<long> {
        public const byte PrefixChar = (byte)'i';

        public BInt(long value) : base(value) {
        }

        public override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            stream.WriteVarInt(Value);
        }

        public static implicit operator BInt(long value) => new BInt(value);
    }
}
