using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnlyChain.Network.Objects {
    public sealed class BAddress : BValue<Address> {
        public const byte PrefixChar = (byte)'a';

        public BAddress(in Address value) : base(in value) {
        }

        unsafe public override void Write(Stream stream) {
            stream.WriteByte(PrefixChar);
            fixed (Address* p = &Value) {
                stream.Write(new ReadOnlySpan<byte>(p, sizeof(Address)));
            }
        }

        public static implicit operator BAddress(in Address value) => new BAddress(value);
    }
}
