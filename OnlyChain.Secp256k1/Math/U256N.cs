using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
#pragma warning disable CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()

namespace OnlyChain.Secp256k1.Math {
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct U256N {
        public static readonly U256 N = new U256("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141");

        public readonly U256 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256N(in U256 value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256N(ReadOnlySpan<byte> u256Bytes) {
            Value = new U256(u256Bytes, bigEndian: true);
            u256_norm_n(ref Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator U256(in U256N @this) => @this.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator U256N(in U256 value) => new U256N(value);


        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator +(in U256N a, in U256N b) {
            u256_add_n(a, b, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator -(in U256N a, in U256N b) {
            u256_sub_n(a, b, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator *(in U256N a, in U256N b) {
            u256_mul_n(a, b, out var r);
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator /(in U256N a, in U256N b) {
            return a * ~b;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator -(in U256N a) {
            u256_neg_n(a, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256N operator ~(in U256N a) {
            if (a.Value.IsZero) throw new DivideByZeroException();
            u256_inv_n(a, out var r);
            return r;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in U256N a, in U256N b) {
            return a.Value == b.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in U256N a, in U256N b) {
            return a.Value != b.Value;
        }

        public override string ToString() {
            return Value.ToString();
        }




#pragma warning disable IDE1006 // 命名样式

        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_mul_n(in U256N a, in U256N b, out U256N result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_square_n(in U256N a, out U256N result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_add_n(in U256N a, in U256N b, out U256N result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_sub_n(in U256N a, in U256N b, out U256N result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_norm_n(ref U256 a);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_neg_n(in U256N a, out U256N result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_inv_n(in U256N a, out U256N result);

#pragma warning restore IDE1006 // 命名样式
    }
}

#pragma warning restore CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
#pragma warning restore CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)