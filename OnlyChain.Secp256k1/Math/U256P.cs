using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
#pragma warning disable CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()

namespace OnlyChain.Secp256k1.Math {
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct U256P {
        public static readonly U256 P = new U256("fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f");

        public readonly U256 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public U256P(U256 value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator U256(in U256P @this) => @this.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator U256P(in U256 value) => new U256P(value);


        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator +(in U256P a, in U256P b) {
            u256_add_p(a, b, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator +(in U256P a, ulong b) {
            u256_add_u64_p(a, b, out var r);
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator +(ulong a, in U256P b) => b + a;

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator -(in U256P a, in U256P b) {
            u256_sub_p(a, b, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator *(in U256P a, in U256P b) {
            u256_mul_p(a, b, out var r);
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator *(in U256P a, ulong b) {
            return b * a;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator *(ulong a, in U256P b) {
            if (a == 2) {
                return b + b;
            } else {
                u256_mul_u64_p(b, a, out var r);
                return r;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator /(in U256P a, in U256P b) {
            return a * ~b;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator -(in U256P a) {
            u256_neg_p(a, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator ~(in U256P a) {
            if (a.Value.IsZero) throw new DivideByZeroException();
            u256_inv_p(a, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P Sqrt(in U256P a) {
            if (!u256_sqrt_p(a, out var r)) throw new ArithmeticException("没有平方根");
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256P operator ^(in U256P a, int pow) {
            switch (pow) {
                case 2: {
                    u256_square_p(a, out var r);
                    return r;
                }
                case 3: {
                    u256_square_p(a, out var r);
                    return a * r;
                }
                default: throw new NotSupportedException("只支持2次幂和3次幂");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in U256P a, in U256P b) {
            return a.Value == b.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in U256P a, in U256P b) {
            return a.Value != b.Value;
        }

        public override string ToString() {
            return Value.ToString();
        }



#pragma warning disable IDE1006 // 命名样式

        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_mul_p(in U256P a, in U256P b, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_mul_u64_p(in U256P a, ulong b, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_square_p(in U256P a, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_add_p(in U256P a, in U256P b, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_add_u64_p(in U256P a, ulong b, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_sub_p(in U256P a, in U256P b, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_norm_p(ref U256P a);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_neg_p(in U256P a, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern void u256_inv_p(in U256P a, out U256P result);
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        public static extern bool u256_sqrt_p(in U256P a, out U256P result);

#pragma warning restore IDE1006 // 命名样式
    }
}

#pragma warning restore CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
#pragma warning restore CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)