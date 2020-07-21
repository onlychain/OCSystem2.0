using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnlyChain.Network.Objects {
    public sealed class BBool : BObject {
        public const byte FalseChar = (byte)'0';
        public const byte TrueChar = (byte)'1';
        public static readonly BBool False = new BBool();
        public static readonly BBool True = new BBool();

        private BBool() {
        }

        public override void Write(Stream stream) {
            stream.WriteByte(this ? TrueChar : FalseChar);
        }

        public static implicit operator BBool(bool value) => value ? True : False;
        public static implicit operator bool(BBool @this) => ReferenceEquals(@this, True);

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            return this ? obj is true : obj is false;
        }

        public override int GetHashCode() => this ? true.GetHashCode() : false.GetHashCode();

        public override string ToString() => this ? true.ToString() : false.ToString();
    }
}
