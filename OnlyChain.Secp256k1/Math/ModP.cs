using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using static OnlyChain.Secp256k1.Math.U256Math;

namespace OnlyChain.Secp256k1.Math {
    internal static class ModP {
        public static readonly U256 P = new U256("fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 U256(ReadOnlySpan<byte> bytes, bool bigEndian = false) {
            var ret = new U256(bytes, bigEndian);
            u256_norm_p(ref ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Add(in U256 a, in U256 b) {
            u256_add_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Sub(in U256 a, in U256 b) {
            u256_sub_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Mul(in U256 a, in U256 b) {
            u256_mul_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Mul(in U256 a, ulong b) {
            u256_mul_u64_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Mul(ulong a, in U256 b) {
            u256_mul_u64_p(b, a, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Div(in U256 a, in U256 b) {
            if (b.IsZero) throw new DivideByZeroException();
            if (a.IsZero) return Math.U256.Zero;
            u256_inv_p(b, out var ret);
            u256_mul_p(a, ret, out ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Neg(in U256 a) {
            u256_neg_p(a, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Pow(in U256 a, in U256 b) {
            u256_pow_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Pow(in U256 a, ulong b) {
            u256_pow_u64_p(a, b, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Sqrt(in U256 a) {
            if (!u256_sqrt_p(a, out var ret)) throw new ArgumentException("没有平方根", nameof(a));
            //u256_fast_sqrt_p(a, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Square(in U256 a) {
            u256_square_p(a, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 Inverse(in U256 a) {
            if (a.IsZero) throw new DivideByZeroException();
            u256_inv_p(a, out var ret);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Add(in Fraction a, in Fraction b) {
            if (a.Den == b.Den) {
                return new Fraction(Add(a.Num, b.Num), a.Den);
            } else {
                return new Fraction(Add(Mul(a.Num, b.Den), Mul(a.Den, b.Num)), Mul(a.Den, b.Den));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Add(in U256 a, in Fraction b) {
            if (b.Den == Math.U256.One) {
                return new Fraction(Add(a, b.Num), Math.U256.One);
            } else {
                return new Fraction(Add(Mul(a, b.Den), b.Num), b.Den);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Add(in Fraction a, in U256 b) {
            if (a.Den == Math.U256.One) {
                return new Fraction(Add(a.Num, b), Math.U256.One);
            } else {
                return new Fraction(Add(a.Num, Mul(a.Den, b)), a.Den);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Sub(in Fraction a, in Fraction b) {
            if (a.Den == b.Den) {
                return new Fraction(Sub(a.Num, b.Num), a.Den);
            } else {
                return new Fraction(Sub(Mul(a.Num, b.Den), Mul(a.Den, b.Num)), Mul(a.Den, b.Den));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Sub(in U256 a, in Fraction b) {
            if (b.Den == Math.U256.One) {
                return new Fraction(Sub(a, b.Num), Math.U256.One);
            } else {
                return new Fraction(Sub(Mul(a, b.Den), b.Num), b.Den);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Sub(in Fraction a, in U256 b) {
            if (a.Den == Math.U256.One) {
                return new Fraction(Sub(a.Num, b), Math.U256.One);
            } else {
                return new Fraction(Sub(a.Num, Mul(a.Den, b)), a.Den);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Neg(in Fraction a) {
            return new Fraction(Neg(a.Num), a.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul(in Fraction a, in Fraction b) {
            if (a.IsZero || b.IsZero) return Fraction.Zero;
            return new Fraction(Mul(a.Num, b.Num), Mul(a.Den, b.Den));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul(in Fraction a, in U256 b) {
            if (a.IsZero || b.IsZero) return Fraction.Zero;
            return new Fraction(Mul(a.Num, b), a.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul(in U256 a, in Fraction b) {
            if (a.IsZero || b.IsZero) return Fraction.Zero;
            return new Fraction(Mul(a, b.Num), b.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul(in Fraction a, ulong b) {
            if (a.IsZero || b == 0) return Fraction.Zero;
            return new Fraction(Mul(a.Num, b), a.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul(ulong a, in Fraction b) {
            if (a == 0 || b.IsZero) return Fraction.Zero;
            return new Fraction(Mul(a, b.Num), b.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Div(in Fraction a, in Fraction b) {
            if (b.IsZero) throw new DivideByZeroException();
            if (a.IsZero) return Fraction.Zero;
            return new Fraction(Mul(a.Num, b.Den), Mul(a.Den, b.Num));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Square(in Fraction a) {
            return new Fraction(Square(a.Num), Square(a.Den));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equal(in Fraction a, in Fraction b) {
            return Mul(a.Num, b.Den) == Mul(a.Den, b.Num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equal(in Fraction a, in U256 b) {
            return a.Num == Mul(a.Den, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equal(in U256 a, in Fraction b) {
            return Mul(a, b.Den) == b.Num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U256 ToU256(in Fraction a) {
            return Div(a.Num, a.Den);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fraction Mul2(in Fraction a) {
            return new Fraction(Add(a.Num, a.Num), a.Den);
        }

        public static Point Mul2(in Point p) {
            var lambda = Div(Mul(Square(p.X), 3), Mul2(p.Y));
            var x = Sub(Square(lambda), Mul2(p.X));
            var y = Sub(Mul(lambda, Sub(p.X, x)), p.Y);
            return new Point(x, y);
        }

        public static Point Add(in Point p1, in Point p2) {
            if (p1.IsZero) return p2;
            if (p2.IsZero) return p1;
            if (Equal(p1.X, p2.X)) {
                if (Equal(p1.Y, p2.Y)) {
                    return Mul2(p1);
                }
                return Point.Zero;
            } else {
                var lambda = Div(Sub(p2.Y, p1.Y), Sub(p2.X, p1.X));
                var x = Sub(Square(lambda), Add(p1.X, p2.X));
                var y = Sub(Mul(lambda, Sub(p1.X, x)), p1.Y);
                return new Point(x, y);
            }
        }

        public static Point FastAdd(in Point p1, in U256 x2, in U256 y2) {
            if (p1.IsZero) return new Point(x2, y2);
            if (Equal(p1.X, x2)) {
                if (Equal(p1.Y, y2)) {
                    return Mul2(p1);
                }
                return Point.Zero;
            } else {
                var lambda = Div(Sub(y2, p1.Y), Sub(x2, p1.X));
                var x = Sub(Square(lambda), Add(p1.X, x2));
                var y = Sub(Mul(lambda, Sub(p1.X, x)), p1.Y);
                return new Point(x, y);
            }
        }


        unsafe public static Point Mul(in Point p, U256 n) {
            if (p.IsZero || n.IsZero) return Point.Zero;

            Point ret = Point.Zero, temp = p;
            for (int i = 0; i < 4; i++) {
                ulong v = ((ulong*)&n)[i];
                for (int j = 0; j < 64; j++) {
                    if ((v & (1UL << j)) != 0) {
                        ret = Add(ret, temp);
                    }
                    temp = Mul2(temp);
                }
            }
            new Span<byte>(&n, sizeof(U256)).Clear();
            return ret;
        }

        public static Point Neg(in Point p) {
            return new Point(p.X, Neg(p.Y));
        }

        public static bool Equal(in Point p1, in Point p2) {
            if (p1.IsZero && p2.IsZero) return true;
            if (p1.IsZero || p2.IsZero) return false;
            return Equal(p1.X, p2.X) && Equal(p1.Y, p2.Y);
        }

        public static readonly U256 Seven = 7;

        public static U256 GetY(in U256 x) {
            return Sqrt(Add(Mul(Square(x), x), Seven));
        }

        static readonly U256 Gx = new U256("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798");
        static readonly U256 Gy = new U256("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8");
        public static readonly Point G = new Point(Gx, Gy);
        static readonly U256[,] Gx_Table = new U256[32, 256];
        static readonly U256[,] Gy_Table = new U256[32, 256];

        static ModP() {
            var tempGTable = new Point[256];
            tempGTable[0] = G;
            for (int i = 1; i < 256; i++) {
                tempGTable[i] = Mul2(tempGTable[i - 1]);
            }

            for (int j = 0; j < 32; j++) {
                for (int i = 1; i < 256; i++) {
                    var t = Point.Zero;
                    if ((i & 1) != 0) t = Add(t, tempGTable[j * 8 + 0]);
                    if ((i & 2) != 0) t = Add(t, tempGTable[j * 8 + 1]);
                    if ((i & 4) != 0) t = Add(t, tempGTable[j * 8 + 2]);
                    if ((i & 8) != 0) t = Add(t, tempGTable[j * 8 + 3]);
                    if ((i & 16) != 0) t = Add(t, tempGTable[j * 8 + 4]);
                    if ((i & 32) != 0) t = Add(t, tempGTable[j * 8 + 5]);
                    if ((i & 64) != 0) t = Add(t, tempGTable[j * 8 + 6]);
                    if ((i & 128) != 0) t = Add(t, tempGTable[j * 8 + 7]);
                    Gx_Table[j, i] = ToU256(t.X);
                    Gy_Table[j, i] = ToU256(t.Y);
                }
            }
        }

        unsafe public static Point MulG(U256 n) {
            Point ret = Point.Zero;
            for (int j = 0; j < 32; j++) {
                var i = ((byte*)&n)[j];
                if (i != 0) {
                    ret = FastAdd(ret, Gx_Table[j, i], Gy_Table[j, i]);
                }
            }
            new Span<byte>(&n, sizeof(U256)).Clear();
            return ret;
        }
    }
}
