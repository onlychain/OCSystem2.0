using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Secp256k1.Math {
    unsafe internal static class EllipticCurve {
        public static readonly Point G = new Point(new U256("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798"), new U256("483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8"));

        static readonly Point[,] table = new Point[32, 256];

        static EllipticCurve() {
            JacobianPoint* tempTable = stackalloc JacobianPoint[256];
            tempTable[0] = G;
            for (int i = 1; i < 256; i++) {
                tempTable[i] = JacobianPoint.Double(tempTable[i - 1]);
            }

            byte* graycode = stackalloc byte[256];
            for (int n = 0; n < 8; n++) {
                int offset = 1 << n;
                graycode[offset] = (byte)n;
                for (int i = 1; i < offset; i++) {
                    graycode[offset + i] = graycode[offset - i];
                }
            }

            for (int j = 0; j < 32; j++) {
                var t = JacobianPoint.Zero;
                int index = 0;
                for (int i = 1; i < 256; i++) {
                    int tableIndex = graycode[i];
                    if ((index & (1 << tableIndex)) == 0) {
                        t += tempTable[j * 8 + tableIndex];
                    } else {
                        t -= tempTable[j * 8 + tableIndex];
                    }
                    index ^= 1 << tableIndex;
                    table[j, index] = (Point)t;
                }
            }
        }

        public static JacobianPoint MulG(U256N n) {
            JacobianPoint r = JacobianPoint.Zero;
            fixed (Point* pTable = table) {
                for (int j = 0; j < 32; j++) {
                    var i = ((byte*)&n)[j];
                    if (i != 0) {
                        Native.jpoint_add_point(&r, &pTable[j * 256 + i], &r);
                    }
                }
            }
            return r;
        }

        public static U256P GetY(in U256P x) {
            return U256P.Sqrt((x ^ 3) + 7);
        }
    }
}
