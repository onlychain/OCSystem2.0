using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace OnlyChain.Network.Objects {
    public abstract class BObject : DynamicObject {
        public abstract void Write(System.IO.Stream stream);

        public static implicit operator BObject(bool value) => (BBool)value;
        public static implicit operator BObject(int value) => new BInt(value);
        public static implicit operator BObject(long value) => (BInt)value;
        public static implicit operator BObject(ulong value) => (BUInt)value;
        public static implicit operator BObject(string value) => (BString)value;
        public static implicit operator BObject(byte[] value) => new BBuffer(value);
        public static implicit operator BObject(BObject[] value) => new BList(value);
        public static implicit operator BObject(List<BObject> value) => new BList(value);
        public static implicit operator BObject(Dictionary<string, BObject> value) => (BDict)value;
        public static implicit operator BObject(in Address value) => new BAddress(in value);
    }
}
