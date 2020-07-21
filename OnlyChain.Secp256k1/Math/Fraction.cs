using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlyChain.Secp256k1.Math {
	internal readonly struct Fraction {
		public readonly U256 Num, Den;

		public static readonly Fraction Zero = new Fraction(U256.Zero);
		public static readonly Fraction One = new Fraction(U256.One);

		public bool IsZero {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Num.IsZero;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Fraction(in U256 num, in U256 den) {
			if (den.IsZero) throw new DivideByZeroException();
			(Num, Den) = (num, den);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Fraction(in U256 num) : this(num, U256.One) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Fraction(in U256 val) => new Fraction(val);
	}
}
