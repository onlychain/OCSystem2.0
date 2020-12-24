#include <cinttypes>

#ifndef __clang__
#error Only clang is supported.
#endif

#define ALWAYS_INLINE __attribute__((always_inline))
#define NO_INLINE __attribute__((noinline))

#ifdef _WINDOWS
#define SECP256K1_API extern "C" __declspec(dllexport)
#else
#define SECP256K1_API extern "C" __attribute__((visibility("default")))
#endif

typedef uint64_t u64;
typedef int64_t i64;
typedef __uint128_t u128;

using u192 = unsigned _ExtInt(192);
using u256 = unsigned _ExtInt(256);
using u257 = unsigned _ExtInt(257);
using u320 = unsigned _ExtInt(320);
using u448 = unsigned _ExtInt(448);
using u512 = unsigned _ExtInt(512);

using i256 = signed _ExtInt(256);
using i320 = signed _ExtInt(320);

ALWAYS_INLINE
static void mul(u64 x, u64 y, u64& low, u64& high) {
    u128 t = (u128)x * y;
    low = (u64)t;
    high = t >> 64;
}
ALWAYS_INLINE
static void square(u64 x, u64& low, u64& high) {
    mul(x, x, low, high);
}
ALWAYS_INLINE
static void add(u64 x, u64& y, u64& of1) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
	)" : [y] "+r"(y), [of1]"+r"(of1) : [x] "r"(x));
}
ALWAYS_INLINE
static void add(u64 x, u64& y, u64& of1, u64& of2) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2) : [x] "r"(x));
}
ALWAYS_INLINE
static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
		adcq $0, %[of3]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3) : [x] "r"(x));
}
ALWAYS_INLINE
static void sub(u64 x, u64& y, u64& of1, u64& of2, u64& of3) {
    __asm(R"(
		subq %[x], %[y]
		sbbq $0, %[of1]
		sbbq $0, %[of2]
		sbbq $0, %[of3]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3) : [x] "r"(x));
}
ALWAYS_INLINE
static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3, u64& of4) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
		adcq $0, %[of3]
		adcq $0, %[of4]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3), [of4]"+r"(of4) : [x] "r"(x));
}
ALWAYS_INLINE
static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3, u64& of4, u64& of5) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
		adcq $0, %[of3]
		adcq $0, %[of4]
		adcq $0, %[of5]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3), [of4]"+r"(of4), [of5]"+r"(of5) : [x] "r"(x));
}
ALWAYS_INLINE
static void add_u448_384(u64 x1, u64 x2, u64 x3, u64 x4, u64 x5, u64 x6, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6, u64& y7) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq %[x5], %[y5]
		adcq %[x6], %[y6]
		adcq $0, %[y7]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5), [y6]"+r"(y6), [y7]"+r"(y7) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4), [x5] "r"(x5), [x6] "r"(x6));
}
ALWAYS_INLINE
static void add_u448_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6, u64& y7) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq $0, %[y5]
		adcq $0, %[y6]
		adcq $0, %[y7]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5), [y6]"+r"(y6), [y7]"+r"(y7) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
static void add_u384_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq $0, %[y5]
		adcq $0, %[y6]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5), [y6]"+r"(y6) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
static void add_u384_320(u64 x1, u64 x2, u64 x3, u64 x4, u64 x5, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq %[x5], %[y5]
		adcq $0, %[y6]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5), [y6]"+r"(y6) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4), [x5] "r"(x5));
}
ALWAYS_INLINE
static void add_u320_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq $0, %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
static void add_u320_128(u64 x1, u64 x2, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq $0, %[y3]
		adcq $0, %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2));
}
ALWAYS_INLINE
static void add_u192_128(u64 x1, u64 x2, u64& y1, u64& y2, u64& y3) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq $0, %[y3]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3) : [x1] "r"(x1), [x2] "r"(x2));
}
ALWAYS_INLINE
static void add_u256_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq $0, %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
static void add_u256_128(u64 x1, u64 x2, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq $0, %[y3]
		adcq $0, %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2));
}
ALWAYS_INLINE
static void sub_u256_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq $0, %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
static void add_u320_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
static void sub_u320_u256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq %[x4], %[y4]
		sbbq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
static void sub_u256_u256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq %[x4], %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}


SECP256K1_API
int u256_less_than(const u64 a[4], const u64 b[4]) {
    return reinterpret_cast<const u256&>(*a) < reinterpret_cast<const u256&>(*b);
}

SECP256K1_API
int u256_less_than_equal(const u64 a[4], const u64 b[4]) {
    return reinterpret_cast<const u256&>(*a) <= reinterpret_cast<const u256&>(*b);
}

SECP256K1_API
int u256_great_than(const u64 a[4], const u64 b[4]) {
    return reinterpret_cast<const u256&>(*a) > reinterpret_cast<const u256&>(*b);
}

SECP256K1_API
int u256_great_than_equal(const u64 a[4], const u64 b[4]) {
    return reinterpret_cast<const u256&>(*a) >= reinterpret_cast<const u256&>(*b);
}


ALWAYS_INLINE
static void u320_mod_p_sub(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 negP = 0x1000003d1; // 2^256 - p
    u64 t0 = x1, t1 = x2, t2 = x3, t3 = x4;
    add(negP, t0, t1, t2, t3, x5);
    if (x5 != 0) {
        x1 = t0;
        x2 = t1;
        x3 = t2;
        x4 = t3;
    }
}

ALWAYS_INLINE
static void u320_mod_p(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 negP = 0x1000003d1; // 2^256 - p
    u64 t0, t1;
    mul(x5, negP, t0, t1);
    x5 = 0;
    add_u320_128(t0, t1, x1, x2, x3, x4, x5);
    u320_mod_p_sub(x1, x2, x3, x4, x5);
}

ALWAYS_INLINE
static void u512_mod_p(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5, u64 x6, u64 x7, u64 x8) {
    constexpr u64 negP = 0x1000003d1; // 2^256 - p
/*
               x8            x7            x6            x5
                                                         -p
-------------------------------------------------------------
         H3 <- x8'     H2 <- x7'     H1 <- x6'     H0 <- x5'
-------------------------------------------------------------
 H3            x8'           x7'           x6'           x5'
               H2            H1            H0
*/
    u64 H0, H1, H2, H3;
    mul(x5, negP, x5, H0);
    mul(x6, negP, x6, H1);
    mul(x7, negP, x7, H2);
    mul(x8, negP, x8, H3);
    add_u256_192(H0, H1, H2, x6, x7, x8, H3); // 用 [x5,x6,x7,x8,H3] 存 u320

    add_u320_256(x5, x6, x7, x8, x1, x2, x3, x4, H3);

    u320_mod_p(x1, x2, x3, x4, H3);
}

// u320 mod p到u256，但不一定<p
ALWAYS_INLINE
static u256 u320_to_u256(const u320& a) {
    u257 t257 = (u257)(a >> 256) * (u257)0x1000003d1ULL + (u257)(u256)a;
    t257 = (u257)(t257 >> 256) * (u257)0x1000003d1ULL + (u257)(u256)t257;
    return (u256)t257;
}

// u512 mod p到u256，但不一定<p
ALWAYS_INLINE
static u256 u512_to_u256(const u512& a) {
    u320 t320 = (u320)(a >> 256) * (u320)0x1000003d1ULL + (u320)(u256)a;
    return u320_to_u256(t320);
}

// LLVM不优化a*a的情况（目前最优只需要10次乘法，而LLVM依然采用16次乘法），因此单独实现。
SECP256K1_API
void u256_square_p(const u64 a[4], u64 r[4]) {
    /*
                                                         3             2             1             0
                                                         3             2             1             0
------------------------------------------------------------------------------------------------------
                                                 H03 <- L03    H02 <- L02    H01 <- L01    H00 <- L00
                                   H13 <- L13    H12 <- L12    H11 <- L11    H01 <- L01
                     H23 <- L23    H22 <- L22    H12 <- L12    H02 <- L02
       H33 <- L33    H23 <- L23    H13 <- L13    H03 <- L03
------------------------------------------------------------------------------------------------------
H33           L33           L23           L13           L03           L02           L01           L00
              H23           L23           L22           L12           L11           L01
              H23           H13           L13           L12           L02           H00
                            H22           H03           L03           H01
                            H13           H12           H02           H01
                                          H12           H11
                                          H03           H02
    */

    u64 t[8];
    u64 L00, L01, L02, L03, L11, L12, L13, L22, L23, L33;
    u64 H00, H01, H02, H03, H11, H12, H13, H22, H23, H33;

    square(a[0], L00, H00);
    mul(a[0], a[1], L01, H01);
    mul(a[0], a[2], L02, H02);
    mul(a[0], a[3], L03, H03);
    square(a[1], L11, H11);
    mul(a[1], a[2], L12, H12);
    mul(a[1], a[3], L13, H13);
    square(a[2], L22, H22);
    mul(a[2], a[3], L23, H23);
    square(a[3], L33, H33);

    t[0] = L00;
    t[1] = L01;
    t[2] = L02;
    t[3] = L03;
    t[4] = L13;
    t[5] = L23;
    t[6] = L33;
    t[7] = H33;

    add_u448_384(L01, L11, L12, L22, L23, H23, t[1], t[2], t[3], t[4], t[5], t[6], t[7]);
    add_u448_384(H00, L02, L12, L13, H13, H23, t[1], t[2], t[3], t[4], t[5], t[6], t[7]);
    add_u384_256(H01, L03, H03, H22, t[2], t[3], t[4], t[5], t[6], t[7]);
    add_u384_256(H01, H02, H12, H13, t[2], t[3], t[4], t[5], t[6], t[7]);
    add_u320_128(H11, H12, t[3], t[4], t[5], t[6], t[7]);
    add_u320_128(H02, H03, t[3], t[4], t[5], t[6], t[7]);

    u512_mod_p(t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7]);

    r[0] = t[0];
    r[1] = t[1];
    r[2] = t[2];
    r[3] = t[3];
}



static constexpr u256 P = ((u256)0xffffffffffffffffULL << 192) | ((u256)0xffffffffffffffffULL << 128) | ((u256)0xffffffffffffffffULL << 64) | (u256)0xfffffffefffffc2fULL;
static constexpr u256 negP = (u256)0x1000003d1ULL; // 2^256 - P

ALWAYS_INLINE
static u256 u257_mod_p(const u257& a) {
    u257 t = a + (u257)negP;
    if (t >> 256) {
        return (u256)t;
    } else {
        return (u256)a;
    }
}

SECP256K1_API
void u256_norm_p(u64 a[4]) {
    reinterpret_cast<u256&>(*a) = u257_mod_p((u257)reinterpret_cast<const u256&>(*a));
}

SECP256K1_API
void u256_add_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u257 t257 = (u257)reinterpret_cast<const u256&>(*a) + (u257)reinterpret_cast<const u256&>(*b);
    reinterpret_cast<u256&>(*r) = u257_mod_p(t257);
}

SECP256K1_API
void u256_add_u64_p(const u64 a[4], u64 b, u64 r[4]) {
    u257 t257 = (u257)reinterpret_cast<const u256&>(*a) + (u257)b;
    reinterpret_cast<u256&>(*r) = u257_mod_p(t257);
}

SECP256K1_API
void u256_sub_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u256 ta = reinterpret_cast<const u256&>(*a);
    u256 tb = reinterpret_cast<const u256&>(*b);
    if (ta >= tb) {
        reinterpret_cast<u256&>(*r) = ta - tb;
    } else {
        reinterpret_cast<u256&>(*r) = ta - tb + P;
    }
}

SECP256K1_API
void u256_neg_p(const u64 a[4], u64 r[4]) {
    u256 t = reinterpret_cast<const u256&>(*a);
    if (t) {
        reinterpret_cast<u256&>(*r) = P - t;
    } else {
        reinterpret_cast<u256&>(*r) = (u256)0;
    }
}

ALWAYS_INLINE
static u256 u512_mod_p(const u512& a) {
    u320 t320 = (u320)(a >> 256) * (u320)0x1000003d1ULL + (u320)(u256)a;
    u257 t257 = (u257)(t320 >> 256) * (u257)0x1000003d1ULL + (u257)(u256)t320;
    u256 r = (u256)t257;
    t257 += (u257)0x1000003d1ULL;
    if (t257 >> 256) {
        r = (u256)t257;
    }
    return r;
}

NO_INLINE
static u256 u256_mul_p_noinline(const u256& a, const u256& b) {
    return u512_to_u256((u512)a * (u512)b);
}

NO_INLINE
static u256 u256_square_p_noinline(const u256& a) {
    return u512_to_u256((u512)a * (u512)a);
}

ALWAYS_INLINE
static u256 u256_mul_p(const u256& a, const u256& b) {
    return u512_to_u256((u512)a * (u512)b);
}

ALWAYS_INLINE
static u256 u256_square_p(const u256& a) {
    return u512_to_u256((u512)a * (u512)a);
}

SECP256K1_API
void u256_mul_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u512 t512 = (u512)reinterpret_cast<const u256&>(*a) * (u512)reinterpret_cast<const u256&>(*b);
    reinterpret_cast<u256&>(*r) = u512_mod_p(t512);
}

SECP256K1_API
void u256_mul_u64_p(const u64 a[4], u64 b, u64 r[4]) {
    u320 t320 = (u320)reinterpret_cast<const u256&>(*a) * (u320)b;
    reinterpret_cast<u256&>(*r) = u512_mod_p(t320);
}

SECP256K1_API
void u256_inv_p(const u64 a[4], u64 r[4]) {
    u256 x = reinterpret_cast<const u256&>(*a);

    u256 t10 = x;
    t10 = u256_square_p_noinline(t10);

    u256 t100 = t10;
    t100 = u256_square_p_noinline(t100);

    u256 t101 = x;
    t101 = u256_mul_p_noinline(t101, t100);

    u256 t111 = t10;
    t111 = u256_mul_p_noinline(t111, t101);

    u256 t1110 = t111;
    t1110 = u256_square_p_noinline(t1110);

    u256 t111000 = t1110;
    for (int i = 0; i < 2; i++) {
        t111000 = u256_square_p_noinline(t111000);
    }

    u256 t111111 = t111;
    t111111 = u256_mul_p_noinline(t111111, t111000);

    u256 i13 = t111111;
    for (int i = 0; i < 4; i++) {
        i13 = u256_square_p_noinline(i13);
    }
    i13 = u256_mul_p_noinline(i13, t1110);

    u256 x12 = i13;
    for (int i = 0; i < 2; i++) {
        x12 = u256_square_p_noinline(x12);
    }
    x12 = u256_mul_p_noinline(x12, t111);

    u256 x22 = x12;
    for (int i = 0; i < 10; i++) {
        x22 = u256_square_p_noinline(x22);
    }
    x22 = u256_mul_p_noinline(x22, i13);
    x22 = u256_mul_p_noinline(x22, x);

    u256 i29 = x22;
    i29 = u256_square_p_noinline(i29);

    u256 i31 = i29;
    for (int i = 0; i < 2; i++) {
        i31 = u256_square_p_noinline(i31);
    }

    u256 i54 = i31;
    for (int i = 0; i < 22; i++) {
        i54 = u256_square_p_noinline(i54);
    }
    i54 = u256_mul_p_noinline(i54, i31);

    u256 i122 = i54;
    for (int i = 0; i < 20; i++) {
        i122 = u256_square_p_noinline(i122);
    }
    i122 = u256_mul_p_noinline(i122, i29);
    for (int i = 0; i < 46; i++) {
        i122 = u256_square_p_noinline(i122);
    }
    i122 = u256_mul_p_noinline(i122, i54);

    u256 x223 = i122;
    for (int i = 0; i < 110; i++) {
        x223 = u256_square_p_noinline(x223);
    }
    x223 = u256_mul_p_noinline(x223, i122);
    x223 = u256_mul_p_noinline(x223, t111);

    u256 i269 = x223;
    for (int i = 0; i < 23; i++) {
        i269 = u256_square_p_noinline(i269);
    }
    i269 = u256_mul_p_noinline(i269, x22);
    for (int i = 0; i < 7; i++) {
        i269 = u256_square_p_noinline(i269);
    }
    i269 = u256_mul_p_noinline(i269, t101);
    for (int i = 0; i < 3; i++) {
        i269 = u256_square_p_noinline(i269);
    }

    u256 result = t101;
    result = u256_mul_p_noinline(result, i269);

    reinterpret_cast<u256&>(*r) = u257_mod_p((u257)result);
}

SECP256K1_API
int u256_sqrt_p(const u64 a[4], u64 r[4]) {
    u256 x = reinterpret_cast<const u256&>(*a);

    u256 t10 = x;
    t10 = u256_square_p_noinline(t10);

    u256 t11 = x;
    t11 = u256_mul_p_noinline(t11, t10);

    u256 t1100 = t11;
    for (int i = 0; i < 2; i++) {
        t1100 = u256_square_p_noinline(t1100);
    }

    u256 t1111 = t11;
    t1111 = u256_mul_p_noinline(t1111, t1100);

    u256 t11110 = t1111;
    t11110 = u256_square_p_noinline(t11110);

    u256 t11111 = x;
    t11111 = u256_mul_p_noinline(t11111, t11110);

    u256 t1111100 = t11111;
    for (int i = 0; i < 2; i++) {
        t1111100 = u256_square_p_noinline(t1111100);
    }

    u256 t1111111 = t11;
    t1111111 = u256_mul_p_noinline(t1111111, t1111100);

    u256 x11 = t1111111;
    for (int i = 0; i < 4; i++) {
        x11 = u256_square_p_noinline(x11);
    }
    x11 = u256_mul_p_noinline(x11, t1111);

    u256 x22 = x11;
    for (int i = 0; i < 11; i++) {
        x22 = u256_square_p_noinline(x22);
    }
    x22 = u256_mul_p_noinline(x22, x11);

    u256 x27 = x22;
    for (int i = 0; i < 5; i++) {
        x27 = u256_square_p_noinline(x27);
    }
    x27 = u256_mul_p_noinline(x27, t11111);

    u256 x54 = x27;
    for (int i = 0; i < 27; i++) {
        x54 = u256_square_p_noinline(x54);
    }
    x54 = u256_mul_p_noinline(x54, x27);

    u256 x108 = x54;
    for (int i = 0; i < 54; i++) {
        x108 = u256_square_p_noinline(x108);
    }
    x108 = u256_mul_p_noinline(x108, x54);

    u256 x216 = x108;
    for (int i = 0; i < 108; i++) {
        x216 = u256_square_p_noinline(x216);
    }
    x216 = u256_mul_p_noinline(x216, x108);

    u256 x223 = x216;
    for (int i = 0; i < 7; i++) {
        x223 = u256_square_p_noinline(x223);
    }
    x223 = u256_mul_p_noinline(x223, t1111111);

    u256 i266 = x223;
    for (int i = 0; i < 23; i++) {
        i266 = u256_square_p_noinline(i266);
    }
    i266 = u256_mul_p_noinline(i266, x22);
    for (int i = 0; i < 5; i++) {
        i266 = u256_square_p_noinline(i266);
    }
    i266 = u256_mul_p_noinline(i266, x);
    for (int i = 0; i < 3; i++) {
        i266 = u256_square_p_noinline(i266);
    }

    u256 t = u256_mul_p_noinline(t11, i266);
    u256 result = u256_mul_p_noinline(t, x);
    u256 cond = u256_mul_p_noinline(result, t);

    cond = u257_mod_p((u257)cond);
    if (cond == (u256)1) {
        reinterpret_cast<u256&>(*r) = u257_mod_p((u257)result);
        return 1;
    }
    return 0;
}


// =======================================================

static constexpr u256 N = ((u256)0xffffffffffffffffULL << 192) | ((u256)0xfffffffffffffffeULL << 128) | ((u256)0xbaaedce6af48a03bULL << 64) | (u256)0xbfd25e8cd0364141ULL;
static constexpr u256 negN = (((u256)1ULL << 128) | ((u256)0x4551231950b75fc4ULL << 64) | (u256)0x402da1732fc9bebfULL); // 2^256 - N

ALWAYS_INLINE
static u256 u257_mod_n(const u257& a) {
    u257 t = a + (u257)negN;
    if (t >> 256) {
        return (u256)t;
    } else {
        return (u256)a;
    }
}

SECP256K1_API
void u256_norm_n(u64 a[4]) {
    reinterpret_cast<u256&>(*a) = u257_mod_n((u257)reinterpret_cast<const u256&>(*a));
}

SECP256K1_API
void u256_add_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    u257 t257 = (u257)reinterpret_cast<const u256&>(*a) + (u257)reinterpret_cast<const u256&>(*b);
    reinterpret_cast<u256&>(*r) = u257_mod_n(t257);
}

SECP256K1_API
void u256_sub_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    u256 ta = reinterpret_cast<const u256&>(*a);
    u256 tb = reinterpret_cast<const u256&>(*b);
    if (ta >= tb) {
        reinterpret_cast<u256&>(*r) = ta - tb;
    } else {
        reinterpret_cast<u256&>(*r) = ta - tb - negN;
    }
}

SECP256K1_API
void u256_neg_n(const u64 a[4], u64 r[4]) {
    u256 t = reinterpret_cast<const u256&>(*a);
    if (t) {
        reinterpret_cast<u256&>(*r) = N - t;
    } else {
        reinterpret_cast<u256&>(*r) = (u256)0;
    }
}

ALWAYS_INLINE
static u256 u512_mod_n(const u512& a) {
    u448 t448 = (u448)(a >> 256) * (u448)negN + (u448)(u256)a;
    u320 t320 = (u320)(t448 >> 256) * (u320)negN + (u320)(u256)t448;
    t320 = (u320)(t320 >> 256) * (u320)negN + (u320)(u256)t320;
    return u257_mod_n((u257)t320);
}

ALWAYS_INLINE
static u256 u512_to_u256_n(const u512& a) {
    u448 t448 = (u448)(a >> 256) * (u448)negN + (u448)(u256)a; // u512 to u448
    u320 t320 = (u320)(t448 >> 256) * (u320)negN + (u320)(u256)t448; // u448 to u320
    u256 t256 = (u256)(t320 >> 256) * (u256)negN + (u256)t320; // u320 to u256
    return t256;
}

NO_INLINE
static u256 u256_mul_n(const u256& a, const u256& b) {
    u512 t512 = (u512)a * (u512)b;
    return u512_to_u256_n(t512);
}

NO_INLINE
static u256 u256_square_n(const u256& a) {
    u512 t512 = (u512)a * (u512)a;
    return u512_to_u256_n(t512);
}

SECP256K1_API NO_INLINE
void u256_mul_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    u512 t512 = (u512)reinterpret_cast<const u256&>(*a) * (u512)reinterpret_cast<const u256&>(*b);
    reinterpret_cast<u256&>(*r) = u512_mod_n(t512);
}

SECP256K1_API
void u256_mul_u64_n(const u64 a[4], u64 b, u64 r[4]) {
    u320 t320 = (u320)reinterpret_cast<const u256&>(*a) * (u320)b;
    u257 t257 = (u257)(t320 >> 256) * (u257)negN + (u257)(u256)t320;
    reinterpret_cast<u256&>(*r) = u257_mod_n(t257);
}

SECP256K1_API NO_INLINE
void u256_square_n(const u64 a[4], u64 r[4]) {
    u512 t512 = (u512)reinterpret_cast<const u256&>(*a) * (u512)reinterpret_cast<const u256&>(*a);
    reinterpret_cast<u256&>(*r) = u512_mod_n(t512);
}

SECP256K1_API NO_INLINE
void u256_inv_n(const u64 a[4], u64 r[4]) {
    u256 x = reinterpret_cast<const u256&>(*a);

    u256 t10 = x;
    t10 = u256_square_n(t10);

    u256 t11 = x;
    t11 = u256_mul_n(t11, t10);

    u256 t101 = t10;
    t101 = u256_mul_n(t101, t11);

    u256 t111 = t10;
    t111 = u256_mul_n(t111, t101);

    u256 t1001 = t10;
    t1001 = u256_mul_n(t1001, t111);

    u256 t1011 = t10;
    t1011 = u256_mul_n(t1011, t1001);

    u256 t1101 = t10;
    t1101 = u256_mul_n(t1101, t1011);

    u256 t110100 = t1101;
    for (int i = 0; i < 2; i++) {
        t110100 = u256_square_n(t110100);
    }

    u256 t111111 = t1011;
    t111111 = u256_mul_n(t111111, t110100);

    u256 t1111110 = t111111;
    t1111110 = u256_square_n(t1111110);

    u256 t1111111 = x;
    t1111111 = u256_mul_n(t1111111, t1111110);

    u256 t11111110 = t1111111;
    t11111110 = u256_square_n(t11111110);

    u256 t11111111 = x;
    t11111111 = u256_mul_n(t11111111, t11111110);

    u256 i17 = t11111111;
    for (int i = 0; i < 3; i++) {
        i17 = u256_square_n(i17);
    }

    u256 i19 = i17;
    for (int i = 0; i < 2; i++) {
        i19 = u256_square_n(i19);
    }

    u256 i20 = i19;
    i20 = u256_square_n(i20);

    u256 i21 = i20;
    i21 = u256_square_n(i21);

    u256 i39 = i21;
    for (int i = 0; i < 7; i++) {
        i39 = u256_square_n(i39);
    }
    i39 = u256_mul_n(i39, i20);
    for (int i = 0; i < 9; i++) {
        i39 = u256_square_n(i39);
    }
    i39 = u256_mul_n(i39, i21);

    u256 i73 = i39;
    for (int i = 0; i < 6; i++) {
        i73 = u256_square_n(i73);
    }
    i73 = u256_mul_n(i73, i19);
    for (int i = 0; i < 26; i++) {
        i73 = u256_square_n(i73);
    }
    i73 = u256_mul_n(i73, i39);

    u256 x127 = i73;
    for (int i = 0; i < 4; i++) {
        x127 = u256_square_n(x127);
    }
    x127 = u256_mul_n(x127, i17);
    for (int i = 0; i < 60; i++) {
        x127 = u256_square_n(x127);
    }
    x127 = u256_mul_n(x127, i73);
    x127 = u256_mul_n(x127, t1111111);

    u256 i154 = x127;
    for (int i = 0; i < 5; i++) {
        i154 = u256_square_n(i154);
    }
    i154 = u256_mul_n(i154, t1011);
    for (int i = 0; i < 3; i++) {
        i154 = u256_square_n(i154);
    }
    i154 = u256_mul_n(i154, t101);
    for (int i = 0; i < 4; i++) {
        i154 = u256_square_n(i154);
    }

    u256 i166 = t101;
    i166 = u256_mul_n(i166, i154);
    for (int i = 0; i < 4; i++) {
        i166 = u256_square_n(i166);
    }
    i166 = u256_mul_n(i166, t111);
    for (int i = 0; i < 5; i++) {
        i166 = u256_square_n(i166);
    }
    i166 = u256_mul_n(i166, t1101);

    u256 i181 = i166;
    for (int i = 0; i < 2; i++) {
        i181 = u256_square_n(i181);
    }
    i181 = u256_mul_n(i181, t11);
    for (int i = 0; i < 5; i++) {
        i181 = u256_square_n(i181);
    }
    i181 = u256_mul_n(i181, t111);
    for (int i = 0; i < 6; i++) {
        i181 = u256_square_n(i181);
    }

    u256 i193 = t1101;
    i193 = u256_mul_n(i193, i181);
    for (int i = 0; i < 5; i++) {
        i193 = u256_square_n(i193);
    }
    i193 = u256_mul_n(i193, t1011);
    for (int i = 0; i < 4; i++) {
        i193 = u256_square_n(i193);
    }
    i193 = u256_mul_n(i193, t1101);

    u256 i214 = i193;
    for (int i = 0; i < 3; i++) {
        i214 = u256_square_n(i214);
    }
    i214 = u256_mul_n(i214, x);
    for (int i = 0; i < 6; i++) {
        i214 = u256_square_n(i214);
    }
    i214 = u256_mul_n(i214, t101);
    for (int i = 0; i < 10; i++) {
        i214 = u256_square_n(i214);
    }

    u256 i230 = t111;
    i230 = u256_mul_n(i230, i214);
    for (int i = 0; i < 4; i++) {
        i230 = u256_square_n(i230);
    }
    i230 = u256_mul_n(i230, t111);
    for (int i = 0; i < 9; i++) {
        i230 = u256_square_n(i230);
    }
    i230 = u256_mul_n(i230, t11111111);

    u256 i247 = i230;
    for (int i = 0; i < 5; i++) {
        i247 = u256_square_n(i247);
    }
    i247 = u256_mul_n(i247, t1001);
    for (int i = 0; i < 6; i++) {
        i247 = u256_square_n(i247);
    }
    i247 = u256_mul_n(i247, t1011);
    for (int i = 0; i < 4; i++) {
        i247 = u256_square_n(i247);
    }

    u256 i261 = t1101;
    i261 = u256_mul_n(i261, i247);
    for (int i = 0; i < 5; i++) {
        i261 = u256_square_n(i261);
    }
    i261 = u256_mul_n(i261, t11);
    for (int i = 0; i < 6; i++) {
        i261 = u256_square_n(i261);
    }
    i261 = u256_mul_n(i261, t1101);

    u256 i283 = i261;
    for (int i = 0; i < 10; i++) {
        i283 = u256_square_n(i283);
    }
    i283 = u256_mul_n(i283, t1101);
    for (int i = 0; i < 4; i++) {
        i283 = u256_square_n(i283);
    }
    i283 = u256_mul_n(i283, t1001);
    for (int i = 0; i < 6; i++) {
        i283 = u256_square_n(i283);
    }

    u256 result = x;
    result = u256_mul_n(result, i283);
    for (int i = 0; i < 8; i++) {
        result = u256_square_n(result);
    }
    result = u256_mul_n(result, t111111);

    reinterpret_cast<u256&>(*r) = u257_mod_n(result);
}


// ======================================================

struct jpoint {
    u256 x, y, z;

    constexpr bool isZero() const noexcept {
        return z == (u256)0;
    }
};

struct point {
    u256 x, y;
};

ALWAYS_INLINE
static u256 u320_mod_p(const u320& a) {
    u257 t257 = (u257)(a >> 256) * (u257)0x1000003d1ULL + (u257)(u256)a;
    return u257_mod_p(t257);
}

// u512 mod p到u257
ALWAYS_INLINE
static u257 u512_to_u257_p(const u512& a) {
    u320 t320 = (u320)(a >> 256) * (u320)0x1000003d1ULL + (u320)(u256)a;
    u257 t257 = (u257)(t320 >> 256) * (u257)0x1000003d1ULL + (u257)(u256)t320;
    return t257; // 可以保证结果二进制长度不超过257
}

ALWAYS_INLINE
static u256 u257_to_u256_p(const u257& a) {
    return (u256)((a >> 256) * (u257)0x1000003d1ULL + (u257)(u256)a);
}

ALWAYS_INLINE
static u257 u257_mul_p(const u257& a, const u257& b) {
    u256 a256 = u257_to_u256_p(a);
    u256 b256 = u257_to_u256_p(b);
    return u512_to_u257_p((u512)a256 * (u512)b256);
}

ALWAYS_INLINE
static u257 u257_square_p(const u257& a) {
    u256 a256 = u257_to_u256_p(a);
    return u512_to_u257_p((u512)a256 * (u512)a256);
}

ALWAYS_INLINE
static u257 u320_mul_p(const u320& a, const u320& b) {
    u256 a256 = u320_to_u256(a);
    u256 b256 = u320_to_u256(b);
    return u512_to_u257_p((u512)a256 * (u512)b256);
}

ALWAYS_INLINE
static u257 u320_square_p(const u320& a) {
    u256 a256 = u320_to_u256(a);
    return u512_to_u257_p((u512)a256 * (u512)a256);
}

ALWAYS_INLINE
static u257 u256_fast_mul_p(const u256& a, const u256& b) {
    return u512_to_u257_p((u512)a * (u512)b);
}

ALWAYS_INLINE
static u257 u256_fast_square_p(const u256& a) {
    return u512_to_u257_p((u512)a * (u512)a);
}


ALWAYS_INLINE
static u256 u256_neg_p(const u256& a) {
    if (a) {
        return P - a;
    } else {
        return (u256)0;
    }
}

ALWAYS_INLINE
static u256 u320_neg_p(const u320& a) {
    u257 t257 = (u257)(a >> 256) * (u257)0x1000003d1ULL + (u257)(u256)a;
    u256 t256 = u257_mod_p(t257);
    return u256_neg_p(t256);
}



SECP256K1_API ALWAYS_INLINE
void jpoint_double(const jpoint& a, jpoint& r) {
    if (a.isZero()) { r.z = 0; return; }

    u257 xx = u256_fast_square_p(a.x);
    u256 yy = u257_to_u256_p(u256_fast_square_p(a.y));
    u257 xyy = u256_fast_mul_p(a.x, yy);
    u256 _3xx = u320_to_u256((u320)3 * (u320)xx);
    u320 _4xyy = (u320)4 * (u320)xyy;

    u256 t1 = u256_square_p(_3xx);
    u320 t2 = (u320)2 * (u320)_4xyy;
    u256 t3 = u320_neg_p(t2);
    u256 retX = u257_mod_p((u257)t1 + (u257)t3);

    u320 t4 = _4xyy + (u320)u256_neg_p(retX);
    u256 t5 = u256_mul_p(_3xx, u320_to_u256(t4));
    u320 t6 = (u320)8 * (u320)u256_square_p(yy);
    u257 t7 = (u257)t5 + (u257)u320_neg_p(t6);
    u256 retY = u257_mod_p(t7);

    u257 t8 = (u257)2 * (u257)u256_mul_p(a.y, a.z);
    u256 retZ = u257_mod_p(t8);

    r.x = retX;
    r.y = retY;
    r.z = retZ;
}

SECP256K1_API
void jpoint_add(const jpoint& a, const jpoint& b, jpoint& r) {
    if (a.isZero()) { r = b; return; }
    if (b.isZero()) { r = a; return; }

    u256 aZZ = u256_square_p(a.z);
    u256 bZZ = u256_square_p(b.z);
    u256 u1 = u257_mod_p(u256_fast_mul_p(a.x, bZZ));
    u256 u2 = u257_mod_p(u256_fast_mul_p(b.x, aZZ));
    u256 s1 = u257_mod_p(u256_fast_mul_p(u256_mul_p(a.y, bZZ), b.z));
    u256 s2 = u257_mod_p(u256_fast_mul_p(u256_mul_p(b.y, aZZ), a.z));

    if (u1 != u2) { // a.x != b.x
        u256 h = u257_to_u256_p((u257)u2 + (u257)u256_neg_p(u1));
        u256 s = u257_to_u256_p((u257)s2 + (u257)u256_neg_p(s1));
        u256 hh = u256_square_p(h);
        u256 hhh = u256_mul_p(hh, h);

        hh = u256_mul_p(hh, u1);

        u256 t1 = u256_square_p(s);
        u256 t2 = u256_neg_p(u320_mod_p((u320)hhh + (u320)2 * (u320)hh));
        u256 retX = u257_mod_p((u257)t1 + (u257)t2);

        u256 t3 = u257_to_u256_p((u257)hh + (u257)u256_neg_p(retX));
        u256 t4 = u256_mul_p(s, t3);
        u257 t5 = u256_fast_mul_p(s1, hhh);
        u256 t6 = u256_neg_p(u257_mod_p(t5));
        u256 retY = u257_mod_p((u257)t4 + (u257)t6);

        u256 t7 = u256_mul_p(a.z, b.z);
        u256 retZ = u257_mod_p(u256_fast_mul_p(t7, h));

        r.x = retX;
        r.y = retY;
        r.z = retZ;
    } else if (s1 == s2) { // a == b
        jpoint_double(a, r);
    } else {
        r.z = 0;
    }
}


ALWAYS_INLINE
static void point_double(const point& a, jpoint& r) {
    u257 xx = u256_fast_square_p(a.x);
    u256 yy = u257_to_u256_p(u256_fast_square_p(a.y));
    u257 xyy = u256_fast_mul_p(a.x, yy);
    u256 _3xx = u320_to_u256((u320)3 * (u320)xx);
    u320 _4xyy = (u320)4 * (u320)xyy;

    u256 t1 = u256_square_p(_3xx);
    u320 t2 = (u320)2 * (u320)_4xyy;
    u256 t3 = u320_neg_p(t2);
    u256 retX = u257_mod_p((u257)t1 + (u257)t3);

    u320 t4 = _4xyy + (u320)u256_neg_p(retX);
    u256 t5 = u256_mul_p(_3xx, u320_to_u256(t4));
    u320 t6 = (u320)8 * (u320)u256_square_p(yy);
    u257 t7 = (u257)t5 + (u257)u320_neg_p(t6);
    u256 retY = u257_mod_p(t7);

    u257 t8 = (u257)2 * (u257)a.y;
    u256 retZ = u257_mod_p(t8);

    r.x = retX;
    r.y = retY;
    r.z = retZ;
}

SECP256K1_API
void jpoint_add_point(const jpoint& a, const point& b, jpoint& r) {
    if (a.isZero()) { r = { b.x, b.y, 1 }; return; }

    u256 aZZ = u256_square_p(a.z);
    u256 u1 = a.x;
    u256 u2 = u257_mod_p(u256_fast_mul_p(b.x, aZZ));
    u256 s1 = a.y;
    u256 s2 = u257_mod_p(u256_fast_mul_p(u256_mul_p(b.y, aZZ), a.z));

    if (u1 != u2) { // a.x != b.x
        u256 h = u257_to_u256_p((u257)u2 + (u257)u256_neg_p(u1));
        u256 s = u257_to_u256_p((u257)s2 + (u257)u256_neg_p(s1));
        u256 hh = u256_square_p(h);
        u256 hhh = u256_mul_p(hh, h);

        hh = u256_mul_p(hh, u1);

        u256 t1 = u256_square_p(s);
        u256 t2 = u256_neg_p(u320_mod_p((u320)hhh + (u320)2 * (u320)hh));
        u256 retX = u257_mod_p((u257)t1 + (u257)t2);

        u256 t3 = u257_to_u256_p((u257)hh + (u257)u256_neg_p(retX));
        u256 t4 = u256_mul_p(s, t3);
        u257 t5 = u256_fast_mul_p(s1, hhh);
        u256 t6 = u256_neg_p(u257_mod_p(t5));
        u256 retY = u257_mod_p((u257)t4 + (u257)t6);

        u256 retZ = u257_mod_p(u256_fast_mul_p(a.z, h));

        r.x = retX;
        r.y = retY;
        r.z = retZ;
    } else if (s1 == s2) { // a == b
        point_double(b, r);
    } else {
        r.z = 0;
    }
}

