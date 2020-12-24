using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OnlyChain.Secp256k1.Math {
    
    unsafe internal static class Native {
        public const string Dll = "secp256k1";

#pragma warning disable IDE1006 // 命名样式

        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool u256_less_than(in U256 a, in U256 b);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool u256_less_than_equal(in U256 a, in U256 b);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool u256_great_than(in U256 a, in U256 b);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool u256_great_than_equal(in U256 a, in U256 b);


        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_add(in JacobianPoint a, in JacobianPoint b, out JacobianPoint result);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_add(JacobianPoint* a, JacobianPoint* b, JacobianPoint* result);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_double(in JacobianPoint a, out JacobianPoint result);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_double(JacobianPoint* a, JacobianPoint* result);

        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_add_point(in JacobianPoint a, in Point b, out JacobianPoint result);
        [DllImport(Dll)]
        [SuppressUnmanagedCodeSecurity]
        public static extern void jpoint_add_point(JacobianPoint* a, Point* b, JacobianPoint* result);

#pragma warning restore IDE1006 // 命名样式
    }
}
