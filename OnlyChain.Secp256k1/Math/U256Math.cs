using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OnlyChain.Secp256k1.Math {
    [SuppressUnmanagedCodeSecurity]
    unsafe internal static class U256Math {
        const string Dll = "u256math";

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CA1401 // P/Invokes should not be visible
        [DllImport(Dll)]
        public static extern bool u256_less_than(in U256 a, in U256 b);
        [DllImport(Dll)]
        public static extern bool u256_less_than_equal(in U256 a, in U256 b);
        [DllImport(Dll)]
        public static extern bool u256_great_than(in U256 a, in U256 b);
        [DllImport(Dll)]
        public static extern bool u256_great_than_equal(in U256 a, in U256 b);

        [DllImport(Dll)]
        public static extern void u256_mul_p(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_mul_u64_p(in U256 a, ulong b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_square_p(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_pow_p(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_pow_u64_p(in U256 a, ulong b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_add_p(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_sub_p(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_norm_p(ref U256 a);
        [DllImport(Dll)]
        public static extern void u256_neg_p(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_inv_p(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern bool u256_sqrt_p(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_fast_sqrt_p(in U256 a, out U256 result);

        [DllImport(Dll)]
        public static extern void u256_mul_n(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_square_n(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_add_n(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_sub_n(in U256 a, in U256 b, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_norm_n(ref U256 a);
        [DllImport(Dll)]
        public static extern void u256_neg_n(in U256 a, out U256 result);
        [DllImport(Dll)]
        public static extern void u256_inv_n(in U256 a, out U256 result);
#pragma warning restore IDE1006 // 命名样式
#pragma warning restore CA1401 // P/Invokes should not be visible
    }
}
