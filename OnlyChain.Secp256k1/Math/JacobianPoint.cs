using OnlyChain.Secp256k1.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace OnlyChain.Secp256k1.Math {
    [StructLayout(LayoutKind.Sequential)]
    unsafe internal readonly struct JacobianPoint {
        public readonly static JacobianPoint Zero = default;


        public readonly U256P X, Y, Z;

        public bool IsZero {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Z.Value.IsZero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JacobianPoint(in U256P x, in U256P y) {
            X = x;
            Y = y;
            Z = U256.One;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JacobianPoint(in U256P x, in U256P y, in U256P z) {
            X = x;
            Y = y;
            Z = z;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator +(in JacobianPoint a, in JacobianPoint b) {
            Native.jpoint_add(a, b, out var r);
            return r;
        }


        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator +(in JacobianPoint a, in Point b) {
            Native.jpoint_add_point(a, b, out var r);
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator +(in Point a, in JacobianPoint b) => b + a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator -(in JacobianPoint a, in JacobianPoint b) => a + -b;


        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint Double(in JacobianPoint p) {
            Native.jpoint_double(p, out var r);
            return r;
        }

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static JacobianPoint operator *(in JacobianPoint p, in U256N n) {
            const int W = 5;

            int* wnaf = stackalloc int[256];
            new Span<int>(wnaf, 256).Clear();

            int bitIndex = 0, wnafSize = -1;
            U256 m = n;
            int sign = 1;
            int carry = 0;

            if (U256Bit(&m, 255) != 0) {
                m = -n; // 将n的二进制长度限制在<=255，于是w-NAF的长度<=256
                sign = -1;
            }

            while (bitIndex < 256) {
                if (U256Bit(&m, bitIndex) == carry) {
                    bitIndex++;
                    continue;
                }

                int bits = System.Math.Min(W, 256 - bitIndex);
                int word = U256Bits(&m, bitIndex, bits) + carry;
                carry = (word >> (W - 1)) & 1;
                word -= carry << W;

                wnaf[bitIndex] = sign * word;
                wnafSize = bitIndex;
                bitIndex += bits;
            }


            JacobianPoint* wnafTable = stackalloc JacobianPoint[1 << (W - 2)];
            JacobianPoint* wnafNegTable = stackalloc JacobianPoint[1 << (W - 2)];
            JacobianPoint p2 = Double(p);
            wnafTable[0] = p;
            wnafNegTable[0] = -p;
            for (int i = 1; i < (1 << (W - 2)); i++) {
                Native.jpoint_add(&wnafTable[i - 1], &p2, &wnafTable[i]);
                wnafNegTable[i] = -wnafTable[i];
            }

            JacobianPoint ret = Zero;
            for (int i = wnafSize; i >= 0; i--) {
                Native.jpoint_double(&ret, &ret);
                if (wnaf[i] != 0) {
                    if (wnaf[i] > 0) {
                        Native.jpoint_add(&ret, &wnafTable[wnaf[i] >> 1], &ret);
                    } else {
                        Native.jpoint_add(&ret, &wnafNegTable[-wnaf[i] >> 1], &ret);
                    }
                }
            }
            return ret;


            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static int U256Bit(U256* value, int index) {
                Debug.Assert(index >= 0 && index < 256);

                return (((byte*)value)[index >> 3] >> (index & 7)) & 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static int U256Bits(U256* value, int index, int bits) {
                Debug.Assert(index >= 0 && index + bits <= 256);
                Debug.Assert(bits <= W);

                int byteOffset = index & 7;
                if (byteOffset + bits <= 8) {
                    return (((byte*)value)[index >> 3] >> byteOffset) & ((1 << bits) - 1);
                } else {
                    int low = ((byte*)value)[index >> 3] >> byteOffset;
                    int high = ((byte*)value)[(index >> 3) + 1];
                    return (low | (high << (8 - byteOffset))) & ((1 << bits) - 1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator *(in U256N n, in JacobianPoint p) => p * n;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JacobianPoint operator -(in JacobianPoint p) {
            return new JacobianPoint(p.X, -p.Y, p.Z);
        }

        public Point ToPoint() {
            if (Z.Value.IsZero) {
                return default;
            }

            U256P invZ = ~Z;
            U256P invZ_2 = invZ ^ 2;
            U256P retX = X * invZ_2;
            U256P retY = Y * invZ_2 * invZ;
            return new Point(retX, retY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point(in JacobianPoint @this) => @this.ToPoint();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator JacobianPoint(in Point p) => new JacobianPoint(p.X, p.Y);
    }
}
