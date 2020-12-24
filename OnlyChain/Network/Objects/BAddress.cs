using OnlyChain.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network.Objects {
    public sealed class BAddress : BValue<Bytes<Address>> {
        public const byte PrefixChar = (byte)'a';

        public BAddress(in Bytes<Address> value) : base(in value) {
        }

        unsafe protected override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            fixed (Bytes<Address>* p = &Value) {
                stream.Write(new ReadOnlySpan<byte>(p, sizeof(Bytes<Address>)));
            }
        }

        public static implicit operator BAddress(in Bytes<Address> value) => new BAddress(value);

        public override string ToString() => "@" + Value;
    }
}
