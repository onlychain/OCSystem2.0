using System;
using System.IO;
using OnlyChain.Core;

namespace OnlyChain.Network.Objects {
    [System.Diagnostics.DebuggerDisplay("buffer Count={Buffer.Length}")]
    public sealed class BBuffer : BObject {
        public const byte PrefixChar = (byte)'b';

        public byte[] Buffer { get; }

        public BBuffer(byte[] buffer) => Buffer = buffer;
        public BBuffer(ReadOnlySpan<byte> buffer) => Buffer = buffer.ToArray();

        public override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            stream.WriteVarUInt((ulong)Buffer.Length);
            stream.Write(Buffer);
        }

        public static implicit operator byte[](BBuffer @this) => @this.Buffer;

        public void Deconstruct(out byte[] result) => result = Buffer;
    }
}
