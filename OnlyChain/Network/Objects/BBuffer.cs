using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("buffer Count={Buffer.Length}")]
    public sealed class BBuffer : BObject {
        public const byte PrefixChar = (byte)'b';

        public byte[] Buffer { get; }

        public BBuffer(byte[] buffer) => Buffer = buffer;
        public BBuffer(ReadOnlySpan<byte> buffer) => Buffer = buffer.ToArray();

        public override void Write(ref BWriteArgs args) {
            args.Stream.WriteByte(PrefixChar);
            args.Stream.WriteVarUInt((ulong)Buffer.Length);
            args.Stream.Write(Buffer);
        }

        public static implicit operator byte[](BBuffer @this) => @this.Buffer;

        public void Deconstruct(out byte[] result) => result = Buffer;

        public override string ToString() => "b\"" + Hex.ToString(Buffer) + '"';
    }
}
