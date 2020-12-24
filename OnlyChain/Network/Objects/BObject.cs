using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Network.Objects {
    public abstract class BObject : DynamicObject {
        public void Write(Stream stream) {
            BWriteArgs args = new BWriteArgs {
                Stream = stream,
                SortedKey = false,
            };
            Write(ref args);
        }

        public abstract void Write(ref BWriteArgs args);


        public static implicit operator BObject(bool value) => (BBool)value;
        public static implicit operator BObject(int value) => new BInt(value);
        public static implicit operator BObject(uint value) => new BUInt(value);
        public static implicit operator BObject(long value) => (BInt)value;
        public static implicit operator BObject(ulong value) => (BUInt)value;
        public static implicit operator BObject(string value) => (BString)value;
        public static implicit operator BObject(byte[] value) => new BBuffer(value);
        public static implicit operator BObject(BObject[] value) => new BList(value.ToList());
        public static implicit operator BObject(List<BObject> value) => new BList(value);
        public static implicit operator BObject(Dictionary<string, BObject> value) => (BDict)value;
        public static implicit operator BObject(in Bytes<Address> value) => new BAddress(in value);

        public static explicit operator bool(BObject @this) => (BBool)@this;
        public static explicit operator int(BObject @this) => checked((int)(long)(BInt)@this);
        public static explicit operator long(BObject @this) => (BInt)@this;
        public static explicit operator uint(BObject @this) => checked((uint)(ulong)(BUInt)@this);
        public static explicit operator ulong(BObject @this) => (BUInt)@this;
        public static explicit operator string(BObject @this) => (BString)@this;
        public static explicit operator byte[](BObject @this) => (BBuffer)@this;
        public static explicit operator BObject[](BObject @this) => ((BList)@this).ToArray();
        public static explicit operator List<BObject>(BObject @this) => ((BList)@this).ToList();
        public static explicit operator Dictionary<string, BObject>(BObject @this) => new Dictionary<string, BObject>((BDict)@this);
        public static explicit operator Bytes<Address>(BObject @this) => (BAddress)@this;
    }
}
