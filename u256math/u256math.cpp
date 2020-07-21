#include <cinttypes>

typedef uint64_t u64;

#ifndef __clang__
#define ALWAYS_INLINE
#else
#define ALWAYS_INLINE __attribute__((always_inline))
#endif

typedef __uint128_t u128;

ALWAYS_INLINE
inline static void mul(u64 x, u64 y, u64& low, u64& high) {
    u128 t = (u128)x * y;
    low = (u64)t;
    high = t >> 64;
}
ALWAYS_INLINE
inline static void square(u64 x, u64& low, u64& high) {
    mul(x, x, low, high);
}
ALWAYS_INLINE
inline static void add(u64 x, u64& y, u64& of1) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
	)" : [y] "+r"(y), [of1]"+r"(of1) : [x] "r"(x));
}
ALWAYS_INLINE
inline static void add(u64 x, u64& y, u64& of1, u64& of2) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2) : [x] "r"(x));
}
ALWAYS_INLINE
inline static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
		adcq $0, %[of3]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3) : [x] "r"(x));
}
ALWAYS_INLINE
inline static void sub(u64 x, u64& y, u64& of1, u64& of2, u64& of3) {
    __asm(R"(
		subq %[x], %[y]
		sbbq $0, %[of1]
		sbbq $0, %[of2]
		sbbq $0, %[of3]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3) : [x] "r"(x));
}
ALWAYS_INLINE
inline static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3, u64& of4) {
    __asm(R"(
		addq %[x], %[y]
		adcq $0, %[of1]
		adcq $0, %[of2]
		adcq $0, %[of3]
		adcq $0, %[of4]
	)" : [y] "+r"(y), [of1]"+r"(of1), [of2]"+r"(of2), [of3]"+r"(of3), [of4]"+r"(of4) : [x] "r"(x));
}
ALWAYS_INLINE
inline static void add(u64 x, u64& y, u64& of1, u64& of2, u64& of3, u64& of4, u64& of5) {
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
inline static void add_u448_384(u64 x1, u64 x2, u64 x3, u64 x4, u64 x5, u64 x6, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6, u64& y7) {
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
inline static void add_u448_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6, u64& y7) {
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
inline static void add_u384_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6) {
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
inline static void add_u384_320(u64 x1, u64 x2, u64 x3, u64 x4, u64 x5, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5, u64& y6) {
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
inline static void add_u320_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq $0, %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
inline static void add_u320_128(u64 x1, u64 x2, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq $0, %[y3]
		adcq $0, %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2));
}
ALWAYS_INLINE
inline static void add_u256_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq $0, %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
inline static void sub_u256_192(u64 x1, u64 x2, u64 x3, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq $0, %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3));
}
ALWAYS_INLINE
inline static void add_u320_256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		addq %[x1], %[y1]
		adcq %[x2], %[y2]
		adcq %[x3], %[y3]
		adcq %[x4], %[y4]
		adcq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
inline static void sub_u320_u256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4, u64& y5) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq %[x4], %[y4]
		sbbq $0, %[y5]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4), [y5]"+r"(y5) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}
ALWAYS_INLINE
inline static void sub_u256_u256(u64 x1, u64 x2, u64 x3, u64 x4, u64& y1, u64& y2, u64& y3, u64& y4) {
    __asm(R"(
		subq %[x1], %[y1]
		sbbq %[x2], %[y2]
		sbbq %[x3], %[y3]
		sbbq %[x4], %[y4]
	)" : [y1] "+r"(y1), [y2]"+r"(y2), [y3]"+r"(y3), [y4]"+r"(y4) : [x1] "r"(x1), [x2] "r"(x2), [x3] "r"(x3), [x4] "r"(x4));
}


extern "C" __declspec(dllexport) ALWAYS_INLINE
inline int u256_less_than(const u64 a[4], const u64 b[4]) {
    uint8_t ret;
    __asm(R"(
		subq %[b0], %[a0]
		sbbq %[b1], %[a1]
		sbbq %[b2], %[a2]
		sbbq %[b3], %[a3]
		setbb %[ret]
	)" : [ret] "=r"(ret) : [a0] "r"(a[0]), [a1]"r"(a[1]), [a2]"r"(a[2]), [a3]"r"(a[3]), [b0]"m"(b[0]), [b1]"m"(b[1]), [b2]"m"(b[2]), [b3]"m"(b[3]));
    return ret;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline int u256_less_than_equal(const u64 a[4], const u64 b[4]) {
    uint8_t ret;
    __asm(R"(
		subq %[b0], %[a0]
		sbbq %[b1], %[a1]
		sbbq %[b2], %[a2]
		sbbq %[b3], %[a3]
		setbeb %[ret]
	)" : [ret] "=r"(ret) : [a0] "r"(a[0]), [a1]"r"(a[1]), [a2]"r"(a[2]), [a3]"r"(a[3]), [b0]"m"(b[0]), [b1]"m"(b[1]), [b2]"m"(b[2]), [b3]"m"(b[3]));
    return ret;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline int u256_great_than(const u64 a[4], const u64 b[4]) {
    return u256_less_than(b, a);
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline int u256_great_than_equal(const u64 a[4], const u64 b[4]) {
    return u256_less_than_equal(b, a);
}


ALWAYS_INLINE
inline static void u320_mod_p_sub(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
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
inline static void u320_mod_p_add(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 negP = 0x1000003d1; // 2^256 - p
    if (x5 == 0) {
        sub(negP, x1, x2, x3, x4);
    }
}

ALWAYS_INLINE
inline static void u320_mod_p(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 negP = 0x1000003d1; // 2^256 - p
    u64 t0, t1;
    mul(x5, negP, t0, t1);
    x5 = 0;
    add_u320_128(t0, t1, x1, x2, x3, x4, x5);
    u320_mod_p_sub(x1, x2, x3, x4, x5);
}

ALWAYS_INLINE
inline static void u512_mod_p(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5, u64 x6, u64 x7, u64 x8) {
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

extern "C" __declspec(dllexport)
inline void u256_mul_u64_p(const u64 a[4], u64 b, u64 r[4]) {
    u64 L0, L1, L2, L3;
    u64 H0, H1, H2, H3;
    mul(a[0], b, L0, H0);
    mul(a[1], b, L1, H1);
    mul(a[2], b, L2, H2);
    mul(a[3], b, L3, H3);
    add_u256_192(H0, H1, H2, L1, L2, L3, H3);

    u320_mod_p(L0, L1, L2, L3, H3);
    r[0] = L0;
    r[1] = L1;
    r[2] = L2;
    r[3] = L3;
}

extern "C" __declspec(dllexport)
void u256_mul_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    /*
                                                         3             2             1             0
                                                         3             2             1             0
------------------------------------------------------------------------------------------------------
                                                 H30 <- L30    H20 <- L20    H10 <- L10    H00 <- L00
                                   H31 <- L31    H21 <- L21    H11 <- L11    H01 <- L01
                     H32 <- L32    H22 <- L22    H12 <- L12    H02 <- L02
       H33 <- L33    H23 <- L23    H13 <- L13    H03 <- L03
------------------------------------------------------------------------------------------------------
H33           L33           L32           L31           L30           L20           L10           L00
              H32           L23           L22           L21           L11           L01
              H23           H31           L13           L12           L02           H00
                            H22           H30           L03           H10
                            H13           H21           H20           H01
                                          H12           H11
                                          H03           H02
    */

    u64 t0, t1, t2, t3, t4, t5, t6, t7;
    u64 L00, L01, L02, L03, L10, L11, L12, L13, L20, L21, L22, L23, L30, L31, L32, L33;
    u64 H00, H01, H02, H03, H10, H11, H12, H13, H20, H21, H22, H23, H30, H31, H32, H33;

    mul(a[0], b[0], L00, H00);
    mul(a[0], b[1], L01, H01);
    mul(a[0], b[2], L02, H02);
    mul(a[0], b[3], L03, H03);
    mul(a[1], b[0], L10, H10);
    mul(a[1], b[1], L11, H11);
    mul(a[1], b[2], L12, H12);
    mul(a[1], b[3], L13, H13);
    mul(a[2], b[0], L20, H20);
    mul(a[2], b[1], L21, H21);
    mul(a[2], b[2], L22, H22);
    mul(a[2], b[3], L23, H23);
    mul(a[3], b[0], L30, H30);
    mul(a[3], b[1], L31, H31);
    mul(a[3], b[2], L32, H32);
    mul(a[3], b[3], L33, H33);

    t0 = L00;
    t1 = L10;
    t2 = L20;
    t3 = L30;
    t4 = L31;
    t5 = L32;
    t6 = L33;
    t7 = H33;

    add_u448_384(L01, L11, L21, L22, L23, H32, t1, t2, t3, t4, t5, t6, t7);
    add_u448_384(H00, L02, L12, L13, H31, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u384_256(H10, L03, H30, H22, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, H20, H21, H13, t2, t3, t4, t5, t6, t7);
    add_u320_128(H11, H12, t3, t4, t5, t6, t7);
    add_u320_128(H02, H03, t3, t4, t5, t6, t7);

    u512_mod_p(t0, t1, t2, t3, t4, t5, t6, t7);

    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport)
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

    u64 t0, t1, t2, t3, t4, t5, t6, t7;
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

    t0 = L00;
    t1 = L01;
    t2 = L02;
    t3 = L03;
    t4 = L13;
    t5 = L23;
    t6 = L33;
    t7 = H33;

    add_u448_384(L01, L11, L12, L22, L23, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u448_384(H00, L02, L12, L13, H13, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, L03, H03, H22, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, H02, H12, H13, t2, t3, t4, t5, t6, t7);
    add_u320_128(H11, H12, t3, t4, t5, t6, t7);
    add_u320_128(H02, H03, t3, t4, t5, t6, t7);

    u512_mod_p(t0, t1, t2, t3, t4, t5, t6, t7);

    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_norm_p(u64 x[4]) {
    u320_mod_p_sub(x[0], x[1], x[2], x[3], 0);
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_add_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u64 t0 = a[0], t1 = a[1], t2 = a[2], t3 = a[3], t4 = 0;
    add_u320_256(b[0], b[1], b[2], b[3], t0, t1, t2, t3, t4);
    u320_mod_p_sub(t0, t1, t2, t3, t4);
    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_sub_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u64 t0 = a[0], t1 = a[1], t2 = a[2], t3 = a[3], t4 = 1;
    sub_u320_u256(b[0], b[1], b[2], b[3], t0, t1, t2, t3, t4);
    u320_mod_p_add(t0, t1, t2, t3, t4);
    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_neg_p(const u64 a[4], u64 r[4]) {
    if (a[0] || a[1] || a[2] || a[3]) {
        u64 t0 = 0xfffffffefffffc2fULL, t1 = ~0ULL, t2 = ~0ULL, t3 = ~0ULL;
        sub_u256_u256(a[0], a[1], a[2], a[3], t0, t1, t2, t3);
        r[0] = t0;
        r[1] = t1;
        r[2] = t2;
        r[3] = t3;
    } else {
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;
        r[3] = 0;
    }
}

ALWAYS_INLINE
inline void u256_pow_part_p(u64 ret[4], u64 temp[4], u64 pow) {
    for (uint32_t i = 0; i < 64; i++) {
        if (pow & (1ULL << i)) {
            u256_mul_p(ret, temp, ret);
        }
        u256_square_p(temp, temp);
    }
}
ALWAYS_INLINE
inline void u256_pow_part_p(u64 ret[4], u64 temp[4], u64 pow, uint32_t bits) {
    for (uint32_t i = 0; i < bits - 1; i++) {
        if (pow & (1ULL << i)) {
            u256_mul_p(ret, temp, ret);
        }
        u256_square_p(temp, temp);
    }
    if (pow & (1ULL << (bits - 1))) {
        u256_mul_p(ret, temp, ret);
    }
}

extern "C" __declspec(dllexport)
void u256_pow_u64_p(const u64 a[4], u64 b, u64 r[4]) {
    u64 ret[] = { 1, 0, 0, 0 }, temp[] = { a[0], a[1], a[2], a[3] };
    if (b) {
        u256_pow_part_p(ret, temp, b, 64 - __lzcnt64(b));
    }
    r[0] = ret[0];
    r[1] = ret[1];
    r[2] = ret[2];
    r[3] = ret[3];
}

extern "C" __declspec(dllexport)
void u256_pow_p(const u64 a[4], const u64 b[4], u64 r[4]) {
    u64 ret[] = { 1, 0, 0, 0 }, temp[] = { a[0], a[1], a[2], a[3] };
    if (b[3]) {
        u256_pow_part_p(ret, temp, b[0]);
        u256_pow_part_p(ret, temp, b[1]);
        u256_pow_part_p(ret, temp, b[2]);
        u256_pow_part_p(ret, temp, b[3], 64 - __lzcnt64(b[3]));
    } else if (b[2]) {
        u256_pow_part_p(ret, temp, b[0]);
        u256_pow_part_p(ret, temp, b[1]);
        u256_pow_part_p(ret, temp, b[2], 64 - __lzcnt64(b[2]));
    } else if (b[1]) {
        u256_pow_part_p(ret, temp, b[0]);
        u256_pow_part_p(ret, temp, b[1], 64 - __lzcnt64(b[1]));
    } else if (b[0]) {
        u256_pow_part_p(ret, temp, b[0], 64 - __lzcnt64(b[0]));
    }
    r[0] = ret[0];
    r[1] = ret[1];
    r[2] = ret[2];
    r[3] = ret[3];
}

extern "C" __declspec(dllexport)
void u256_inv_p(const u64 a[4], u64 r[4]) {
    if (a[0] == 1 && a[1] == 0 && a[2] == 0 && a[3] == 0) {
        r[0] = 1;
        r[1] = 0;
        r[2] = 0;
        r[3] = 0;
        return;
    }

    u64 ret[] = { a[0], a[1], a[2], a[3] }, temp[] = { a[0], a[1], a[2], a[3] };
    u256_square_p(temp, temp);
    u256_square_p(temp, temp); u256_mul_p(ret, temp, ret);
    u256_square_p(temp, temp); u256_mul_p(ret, temp, ret);
    u256_square_p(temp, temp);
    u256_square_p(temp, temp); u256_mul_p(ret, temp, ret);
    u256_square_p(temp, temp);
    u256_square_p(temp, temp);
    u256_square_p(temp, temp);
    u256_square_p(temp, temp);
    for (int i = 0; i < 22; i++) {
        u256_square_p(temp, temp); u256_mul_p(ret, temp, ret);
    }
    u256_square_p(temp, temp);
    for (int i = 0; i < 223; i++) {
        u256_square_p(temp, temp); u256_mul_p(ret, temp, ret);
    }
    r[0] = ret[0];
    r[1] = ret[1];
    r[2] = ret[2];
    r[3] = ret[3];
}

extern "C" __declspec(dllexport)
int u256_sqrt_p(const u64 a[4], u64 r[4]) {
    u64 cond[] = { a[0], a[1], a[2], a[3] }, t[] = { a[0], a[1], a[2], a[3] };
    u256_square_p(t, t); u256_mul_p(cond, t, cond);
    u256_square_p(t, t); u256_mul_p(cond, t, cond);
    u64 ret[] = { t[0], t[1], t[2], t[3] };
    u256_square_p(t, t); u256_mul_p(ret, t, ret);
    u256_square_p(t, t); u256_mul_p(cond, t, cond);
    u256_square_p(t, t);
    u256_square_p(t, t);
    u256_square_p(t, t);
    u256_square_p(t, t); u256_mul_p(ret, t, ret);
    for (int i = 0; i < 21; i++) {
        u256_square_p(t, t);
        u256_mul_p(cond, t, cond);
        u256_mul_p(ret, t, ret);
    }
    u256_square_p(t, t); u256_mul_p(cond, t, cond);
    u256_square_p(t, t); u256_mul_p(ret, t, ret);
    for (int i = 0; i < 222; i++) {
        u256_square_p(t, t);
        u256_mul_p(cond, t, cond);
        u256_mul_p(ret, t, ret);
    }
    u256_square_p(t, t); u256_mul_p(cond, t, cond);
    if (cond[0] == 1 && cond[1] == 0 && cond[2] == 0 && cond[3] == 0) {
        r[0] = ret[0];
        r[1] = ret[1];
        r[2] = ret[2];
        r[3] = ret[3];
        return 1;
    }
    return 0;
}

extern "C" __declspec(dllexport)
void u256_fast_sqrt_p(const u64 a[4], u64 r[4]) {
    u64 t[] = { a[0], a[1], a[2], a[3] };
    u256_square_p(t, t);
    u256_square_p(t, t);
    u64 ret[] = { t[0], t[1], t[2], t[3] };
    u256_square_p(t, t); u256_mul_p(ret, t, ret);
    u256_square_p(t, t);
    u256_square_p(t, t);
    u256_square_p(t, t);
    u256_square_p(t, t);
    for (int i = 0; i < 22; i++) {
        u256_square_p(t, t);
        u256_mul_p(ret, t, ret);
    }
    u256_square_p(t, t);
    for (int i = 0; i < 223; i++) {
        u256_square_p(t, t);
        u256_mul_p(ret, t, ret);
    }
    r[0] = ret[0];
    r[1] = ret[1];
    r[2] = ret[2];
    r[3] = ret[3];
}

ALWAYS_INLINE
inline static void u320_mod_n_sub(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 N0 = 0xbfd25e8cd0364141ULL, N1 = 0xbaaedce6af48a03bULL, N2 = 0xfffffffffffffffeULL;
    constexpr u64 negN0 = 0x402da1732fc9bebfULL, negN1 = 0x4551231950b75fc4ULL, negN2 = 1ULL;

    u64 t0 = x1, t1 = x2, t2 = x3, t3 = x4;
    add_u320_192(negN0, negN1, negN2, t0, t1, t2, t3, x5);
    if (x5 != 0) {
        x1 = t0;
        x2 = t1;
        x3 = t2;
        x4 = t3;
    }
}

ALWAYS_INLINE
inline static void u320_mod_n_add(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5) {
    constexpr u64 N0 = 0xbfd25e8cd0364141ULL, N1 = 0xbaaedce6af48a03bULL, N2 = 0xfffffffffffffffeULL;
    constexpr u64 negN0 = 0x402da1732fc9bebfULL, negN1 = 0x4551231950b75fc4ULL, negN2 = 1ULL;

    if (x5 == 0) {
        sub_u256_192(negN0, negN1, negN2, x1, x2, x3, x4);
    }
}

ALWAYS_INLINE
inline static void u256_mod_n(u64& x1, u64& x2, u64& x3, u64& x4) {
    u320_mod_n_sub(x1, x2, x3, x4, 0);
}

static void u512_mod_n(u64& x1, u64& x2, u64& x3, u64& x4, u64 x5, u64 x6, u64 x7, u64 x8) {
    constexpr u64 N0 = 0xbfd25e8cd0364141ULL, N1 = 0xbaaedce6af48a03bULL, N2 = 0xfffffffffffffffeULL;
    constexpr u64 negN0 = 0x402da1732fc9bebfULL, negN1 = 0x4551231950b75fc4ULL, negN2 = 1ULL;
    constexpr u64 negN0_2 = 0x805b42e65f937d7eULL, negN1_2 = 0x8aa24632a16ebf88ULL, negN2_2 = 2ULL; // -N*2

    /*
                                            3             2             1             0
                                                         $1             1             0
-----------------------------------------------------------------------------------------
                                    H03 <- L03    H02 <- L02    H01 <- L01    H00 <- L00
                      H13 <- L13    H12 <- L12    H11 <- L11    H10 <- L10
                3             2             1             0
-----------------------------------------------------------------------------------------
  H             3            L13           L03           L02           L01           L00
               H13            2            L12           L11           L10
                             H03            1             0            H00
                             H12           H02           H01
                                           H11           H10
    */
    u64 L00, L01, L02, L03, L10, L11, L12, L13;
    u64 H00, H01, H02, H03, H10, H11, H12, H13;

    mul(x5, negN0, L00, H00);
    mul(x6, negN0, L01, H01);
    mul(x7, negN0, L02, H02);
    mul(x8, negN0, L03, H03);
    mul(x5, negN1, L10, H10);
    mul(x6, negN1, L11, H11);
    mul(x7, negN1, L12, H12);
    mul(x8, negN1, L13, H13);

    u64 t0 = L00, t1 = L01, t2 = L02, t3 = L03, t4 = L13, t5 = x8, t6 = 0;
    add_u384_320(L10, L11, L12, x7, H13, t1, t2, t3, t4, t5, t6);
    add_u384_256(H00, x5, x6, H03, t1, t2, t3, t4, t5, t6);
    add_u320_192(H01, H02, H12, t2, t3, t4, t5, t6);
    add_u320_128(H10, H11, t2, t3, t4, t5, t6);

    add_u448_256(x1, x2, x3, x4, t0, t1, t2, t3, t4, t5, t6);

    // t6 = 0 or 1
/*
                              t6            t5            t4
                              $1            n1            n0
-------------------------------------------------------------
                             n0?    H01 <- L01    H00 <- L00
               n1?    H11 <- L11    H10 <- L10
  t6            t5            t4
-------------------------------------------------------------
  t6            t5            t4           L01           L00
               n1?           n0?           L10
               H11           L11           H00
                             H01
                             H10
*/

    mul(t4, negN0, L00, H00);
    mul(t5, negN0, L01, H01);
    mul(t4, negN1, L10, H10);
    mul(t5, negN1, L11, H11);
    // t6  t5  t4  L01  L00
    add_u256_192(L10, -t6 & negN0, -t6 & negN1, L01, t4, t5, t6);
    add_u256_192(H00, L11, H11, L01, t4, t5, t6);
    add(H01, t4, t5, t6);
    add(H10, t4, t5, t6);

    add_u320_256(t0, t1, t2, t3, L00, L01, t4, t5, t6);
    // 0 <= t6 <= 2

    if (t6 == 2) {
        add_u256_192(negN0_2, negN1_2, negN2_2, L00, L01, t4, t5);
    } else {
        if (t6 == 1) {
            add_u256_192(negN0, negN1, negN2, L00, L01, t4, t5);
        }
        u256_mod_n(L00, L01, t4, t5);
    }
    x1 = L00;
    x2 = L01;
    x3 = t4;
    x4 = t5;
}

extern "C" __declspec(dllexport)
void u256_mul_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    /*
                                                         3             2             1             0
                                                         3             2             1             0
------------------------------------------------------------------------------------------------------
                                                 H30 <- L30    H20 <- L20    H10 <- L10    H00 <- L00
                                   H31 <- L31    H21 <- L21    H11 <- L11    H01 <- L01
                     H32 <- L32    H22 <- L22    H12 <- L12    H02 <- L02
       H33 <- L33    H23 <- L23    H13 <- L13    H03 <- L03
------------------------------------------------------------------------------------------------------
H33           L33           L32           L31           L30           L20           L10           L00
              H32           L23           L22           L21           L11           L01
              H23           H31           L13           L12           L02           H00
                            H22           H30           L03           H10
                            H13           H21           H20           H01
                                          H12           H11
                                          H03           H02
    */

    u64 t0, t1, t2, t3, t4, t5, t6, t7;
    u64 L00, L01, L02, L03, L10, L11, L12, L13, L20, L21, L22, L23, L30, L31, L32, L33;
    u64 H00, H01, H02, H03, H10, H11, H12, H13, H20, H21, H22, H23, H30, H31, H32, H33;

    mul(a[0], b[0], L00, H00);
    mul(a[0], b[1], L01, H01);
    mul(a[0], b[2], L02, H02);
    mul(a[0], b[3], L03, H03);
    mul(a[1], b[0], L10, H10);
    mul(a[1], b[1], L11, H11);
    mul(a[1], b[2], L12, H12);
    mul(a[1], b[3], L13, H13);
    mul(a[2], b[0], L20, H20);
    mul(a[2], b[1], L21, H21);
    mul(a[2], b[2], L22, H22);
    mul(a[2], b[3], L23, H23);
    mul(a[3], b[0], L30, H30);
    mul(a[3], b[1], L31, H31);
    mul(a[3], b[2], L32, H32);
    mul(a[3], b[3], L33, H33);

    t0 = L00;
    t1 = L10;
    t2 = L20;
    t3 = L30;
    t4 = L31;
    t5 = L32;
    t6 = L33;
    t7 = H33;

    add_u448_384(L01, L11, L21, L22, L23, H32, t1, t2, t3, t4, t5, t6, t7);
    add_u448_384(H00, L02, L12, L13, H31, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u384_256(H10, L03, H30, H22, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, H20, H21, H13, t2, t3, t4, t5, t6, t7);
    add_u320_128(H11, H12, t3, t4, t5, t6, t7);
    add_u320_128(H02, H03, t3, t4, t5, t6, t7);

    u512_mod_n(t0, t1, t2, t3, t4, t5, t6, t7);

    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}


extern "C" __declspec(dllexport)
void u256_square_n(const u64 a[4], u64 r[4]) {
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

    u64 t0, t1, t2, t3, t4, t5, t6, t7;
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

    t0 = L00;
    t1 = L01;
    t2 = L02;
    t3 = L03;
    t4 = L13;
    t5 = L23;
    t6 = L33;
    t7 = H33;

    add_u448_384(L01, L11, L12, L22, L23, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u448_384(H00, L02, L12, L13, H13, H23, t1, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, L03, H03, H22, t2, t3, t4, t5, t6, t7);
    add_u384_256(H01, H02, H12, H13, t2, t3, t4, t5, t6, t7);
    add_u320_128(H11, H12, t3, t4, t5, t6, t7);
    add_u320_128(H02, H03, t3, t4, t5, t6, t7);

    u512_mod_n(t0, t1, t2, t3, t4, t5, t6, t7);

    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport)
void u256_inv_n(const u64 a[4], u64 r[4]) {
    if (a[0] == 1 && a[1] == 0 && a[2] == 0 && a[3] == 0) {
        r[0] = 1;
        r[1] = 0;
        r[2] = 0;
        r[3] = 0;
        return;
    }

    u64 ret[] = { a[0], a[1], a[2], a[3] }, temp[] = { a[0], a[1], a[2], a[3] };
#define _ do ; while(0)
#define I do { u256_square_n(temp, temp); u256_mul_n(ret, temp, ret); } while (0)
#define O do u256_square_n(temp, temp); while (0)

    _; I; I; I; I; I; O; O;
    I; O; O; O; O; O; I; O;
    O; I; I; O; I; I; O; O;
    O; O; O; O; I; O; I; I;
    O; O; I; I; O; O; O; I;
    O; I; I; I; I; O; I; O;
    O; I; O; O; I; O; I; I;
    I; I; I; I; I; I; O; I;
    I; I; O; I; I; I; O; O;
    O; O; O; O; O; I; O; I;
    O; O; O; I; O; O; I; O;
    I; I; I; I; O; I; O; I;
    O; I; I; O; O; I; I; I;
    O; O; I; I; I; O; I; I;
    O; I; I; I; O; I; O; I;
    O; I; O; I; I; I; O; I;
    O;
    for (int i = 0; i < 127; i++) I;

#undef _
#undef I
#undef O

    r[0] = ret[0];
    r[1] = ret[1];
    r[2] = ret[2];
    r[3] = ret[3];
}

extern "C" __declspec(dllexport)
void u256_norm_n(u64 a[4]) {
    u256_mod_n(a[0], a[1], a[2], a[3]);
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_add_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    u64 t0 = a[0], t1 = a[1], t2 = a[2], t3 = a[3], t4 = 0;
    add_u320_256(b[0], b[1], b[2], b[3], t0, t1, t2, t3, t4);
    u320_mod_n_sub(t0, t1, t2, t3, t4);
    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_sub_n(const u64 a[4], const u64 b[4], u64 r[4]) {
    u64 t0 = a[0], t1 = a[1], t2 = a[2], t3 = a[3], t4 = 1;
    sub_u320_u256(b[0], b[1], b[2], b[3], t0, t1, t2, t3, t4);
    u320_mod_n_add(t0, t1, t2, t3, t4);
    r[0] = t0;
    r[1] = t1;
    r[2] = t2;
    r[3] = t3;
}

extern "C" __declspec(dllexport) ALWAYS_INLINE
inline void u256_neg_n(const u64 a[4], u64 r[4]) {
    if (a[0] || a[1] || a[2] || a[3]) {
        u64 t0 = 0xbfd25e8cd0364141ULL, t1 = 0xbaaedce6af48a03bULL, t2 = ~1ULL, t3 = ~0ULL;
        sub_u256_u256(a[0], a[1], a[2], a[3], t0, t1, t2, t3);
        r[0] = t0;
        r[1] = t1;
        r[2] = t2;
        r[3] = t3;
    } else {
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;
        r[3] = 0;
    }
}