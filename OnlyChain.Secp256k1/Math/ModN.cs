using System;
using System.Runtime.CompilerServices;
using static OnlyChain.Secp256k1.Math.U256Math;

namespace OnlyChain.Secp256k1.Math {
	internal static class ModN {
		public static readonly U256 N = new U256("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141");

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 U256(ReadOnlySpan<byte> bytes, bool bigEndian = false) {
			var ret = new U256(bytes, bigEndian);
			u256_norm_n(ref ret);
			return ret;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Add(in U256 a, in U256 b) {
			u256_add_n(a, b, out var ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Sub(in U256 a, in U256 b) {
			u256_sub_n(a, b, out var ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Mul(in U256 a, in U256 b) {
			u256_mul_n(a, b, out var ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Div(in U256 a, in U256 b) {
			if (b.IsZero) throw new DivideByZeroException();
			if (a.IsZero) return Math.U256.Zero;
			u256_inv_n(b, out var ret);
			u256_mul_n(a, ret, out ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Neg(in U256 a) {
			u256_neg_n(a, out var ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Square(in U256 a) {
			u256_square_n(a, out var ret);
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static U256 Inverse(in U256 a) {
			if (a.IsZero) throw new DivideByZeroException();
			u256_inv_n(a, out var ret);
			return ret;
		}
	}
}
