#include <cinttypes>

#ifdef _WINDOWS
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

#ifndef __clang__
#define ALWAYS_INLINE
#else
#define ALWAYS_INLINE __attribute__((always_inline))
#endif

ALWAYS_INLINE static uint32_t f1(uint32_t x, uint32_t y, uint32_t z) { return x ^ y ^ z; }
ALWAYS_INLINE static uint32_t f2(uint32_t x, uint32_t y, uint32_t z) { return (x & y) | (~x & z); }
ALWAYS_INLINE static uint32_t f3(uint32_t x, uint32_t y, uint32_t z) { return (x | ~y) ^ z; }
ALWAYS_INLINE static uint32_t f4(uint32_t x, uint32_t y, uint32_t z) { return (x & z) | (y & ~z); }
ALWAYS_INLINE static uint32_t f5(uint32_t x, uint32_t y, uint32_t z) { return x ^ (y | ~z); }

ALWAYS_INLINE static uint32_t rotleft(uint32_t x, int i) { return (x << i) | (x >> (32 - i)); }

ALWAYS_INLINE static void Round(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t f, uint32_t x, uint32_t k, int r) {
    a = rotleft(a + f + x + k, r) + e;
    c = rotleft(c, 10);
}

ALWAYS_INLINE static void R11(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f1(b, c, d), x, 0, r); }
ALWAYS_INLINE static void R21(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f2(b, c, d), x, 0x5A827999ul, r); }
ALWAYS_INLINE static void R31(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f3(b, c, d), x, 0x6ED9EBA1ul, r); }
ALWAYS_INLINE static void R41(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f4(b, c, d), x, 0x8F1BBCDCul, r); }
ALWAYS_INLINE static void R51(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f5(b, c, d), x, 0xA953FD4Eul, r); }

ALWAYS_INLINE static void R12(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f5(b, c, d), x, 0x50A28BE6ul, r); }
ALWAYS_INLINE static void R22(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f4(b, c, d), x, 0x5C4DD124ul, r); }
ALWAYS_INLINE static void R32(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f3(b, c, d), x, 0x6D703EF3ul, r); }
ALWAYS_INLINE static void R42(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f2(b, c, d), x, 0x7A6D76E9ul, r); }
ALWAYS_INLINE static void R52(uint32_t& a, uint32_t b, uint32_t& c, uint32_t d, uint32_t e, uint32_t x, int r) { Round(a, b, c, d, e, f1(b, c, d), x, 0, r); }

extern "C" EXPORT void compute_hash(const uint8_t inHash[32], uint8_t outHash[20]) {
    uint32_t m[16];
    m[0] = reinterpret_cast<const uint32_t*>(inHash)[0];
    m[1] = reinterpret_cast<const uint32_t*>(inHash)[1];
    m[2] = reinterpret_cast<const uint32_t*>(inHash)[2];
    m[3] = reinterpret_cast<const uint32_t*>(inHash)[3];
    m[4] = reinterpret_cast<const uint32_t*>(inHash)[4];
    m[5] = reinterpret_cast<const uint32_t*>(inHash)[5];
    m[6] = reinterpret_cast<const uint32_t*>(inHash)[6];
    m[7] = reinterpret_cast<const uint32_t*>(inHash)[7];

    m[8] = 0x80;
    m[9] = 0;
    m[10] = 0;
    m[11] = 0;
    m[12] = 0;
    m[13] = 0;
    m[14] = 256;
    m[15] = 0;

    uint32_t a1 = 0x67452301u, b1 = 0xEFCDAB89u, c1 = 0x98BADCFEu, d1 = 0x10325476u, e1 = 0xC3D2E1F0u;
    uint32_t a2 = 0x67452301u, b2 = 0xEFCDAB89u, c2 = 0x98BADCFEu, d2 = 0x10325476u, e2 = 0xC3D2E1F0u;

    R11(a1, b1, c1, d1, e1, m[0], 11);
    R12(a2, b2, c2, d2, e2, m[5], 8);
    R11(e1, a1, b1, c1, d1, m[1], 14);
    R12(e2, a2, b2, c2, d2, m[14], 9);
    R11(d1, e1, a1, b1, c1, m[2], 15);
    R12(d2, e2, a2, b2, c2, m[7], 9);
    R11(c1, d1, e1, a1, b1, m[3], 12);
    R12(c2, d2, e2, a2, b2, m[0], 11);
    R11(b1, c1, d1, e1, a1, m[4], 5);
    R12(b2, c2, d2, e2, a2, m[9], 13);
    R11(a1, b1, c1, d1, e1, m[5], 8);
    R12(a2, b2, c2, d2, e2, m[2], 15);
    R11(e1, a1, b1, c1, d1, m[6], 7);
    R12(e2, a2, b2, c2, d2, m[11], 15);
    R11(d1, e1, a1, b1, c1, m[7], 9);
    R12(d2, e2, a2, b2, c2, m[4], 5);
    R11(c1, d1, e1, a1, b1, m[8], 11);
    R12(c2, d2, e2, a2, b2, m[13], 7);
    R11(b1, c1, d1, e1, a1, m[9], 13);
    R12(b2, c2, d2, e2, a2, m[6], 7);
    R11(a1, b1, c1, d1, e1, m[10], 14);
    R12(a2, b2, c2, d2, e2, m[15], 8);
    R11(e1, a1, b1, c1, d1, m[11], 15);
    R12(e2, a2, b2, c2, d2, m[8], 11);
    R11(d1, e1, a1, b1, c1, m[12], 6);
    R12(d2, e2, a2, b2, c2, m[1], 14);
    R11(c1, d1, e1, a1, b1, m[13], 7);
    R12(c2, d2, e2, a2, b2, m[10], 14);
    R11(b1, c1, d1, e1, a1, m[14], 9);
    R12(b2, c2, d2, e2, a2, m[3], 12);
    R11(a1, b1, c1, d1, e1, m[15], 8);
    R12(a2, b2, c2, d2, e2, m[12], 6);

    R21(e1, a1, b1, c1, d1, m[7], 7);
    R22(e2, a2, b2, c2, d2, m[6], 9);
    R21(d1, e1, a1, b1, c1, m[4], 6);
    R22(d2, e2, a2, b2, c2, m[11], 13);
    R21(c1, d1, e1, a1, b1, m[13], 8);
    R22(c2, d2, e2, a2, b2, m[3], 15);
    R21(b1, c1, d1, e1, a1, m[1], 13);
    R22(b2, c2, d2, e2, a2, m[7], 7);
    R21(a1, b1, c1, d1, e1, m[10], 11);
    R22(a2, b2, c2, d2, e2, m[0], 12);
    R21(e1, a1, b1, c1, d1, m[6], 9);
    R22(e2, a2, b2, c2, d2, m[13], 8);
    R21(d1, e1, a1, b1, c1, m[15], 7);
    R22(d2, e2, a2, b2, c2, m[5], 9);
    R21(c1, d1, e1, a1, b1, m[3], 15);
    R22(c2, d2, e2, a2, b2, m[10], 11);
    R21(b1, c1, d1, e1, a1, m[12], 7);
    R22(b2, c2, d2, e2, a2, m[14], 7);
    R21(a1, b1, c1, d1, e1, m[0], 12);
    R22(a2, b2, c2, d2, e2, m[15], 7);
    R21(e1, a1, b1, c1, d1, m[9], 15);
    R22(e2, a2, b2, c2, d2, m[8], 12);
    R21(d1, e1, a1, b1, c1, m[5], 9);
    R22(d2, e2, a2, b2, c2, m[12], 7);
    R21(c1, d1, e1, a1, b1, m[2], 11);
    R22(c2, d2, e2, a2, b2, m[4], 6);
    R21(b1, c1, d1, e1, a1, m[14], 7);
    R22(b2, c2, d2, e2, a2, m[9], 15);
    R21(a1, b1, c1, d1, e1, m[11], 13);
    R22(a2, b2, c2, d2, e2, m[1], 13);
    R21(e1, a1, b1, c1, d1, m[8], 12);
    R22(e2, a2, b2, c2, d2, m[2], 11);

    R31(d1, e1, a1, b1, c1, m[3], 11);
    R32(d2, e2, a2, b2, c2, m[15], 9);
    R31(c1, d1, e1, a1, b1, m[10], 13);
    R32(c2, d2, e2, a2, b2, m[5], 7);
    R31(b1, c1, d1, e1, a1, m[14], 6);
    R32(b2, c2, d2, e2, a2, m[1], 15);
    R31(a1, b1, c1, d1, e1, m[4], 7);
    R32(a2, b2, c2, d2, e2, m[3], 11);
    R31(e1, a1, b1, c1, d1, m[9], 14);
    R32(e2, a2, b2, c2, d2, m[7], 8);
    R31(d1, e1, a1, b1, c1, m[15], 9);
    R32(d2, e2, a2, b2, c2, m[14], 6);
    R31(c1, d1, e1, a1, b1, m[8], 13);
    R32(c2, d2, e2, a2, b2, m[6], 6);
    R31(b1, c1, d1, e1, a1, m[1], 15);
    R32(b2, c2, d2, e2, a2, m[9], 14);
    R31(a1, b1, c1, d1, e1, m[2], 14);
    R32(a2, b2, c2, d2, e2, m[11], 12);
    R31(e1, a1, b1, c1, d1, m[7], 8);
    R32(e2, a2, b2, c2, d2, m[8], 13);
    R31(d1, e1, a1, b1, c1, m[0], 13);
    R32(d2, e2, a2, b2, c2, m[12], 5);
    R31(c1, d1, e1, a1, b1, m[6], 6);
    R32(c2, d2, e2, a2, b2, m[2], 14);
    R31(b1, c1, d1, e1, a1, m[13], 5);
    R32(b2, c2, d2, e2, a2, m[10], 13);
    R31(a1, b1, c1, d1, e1, m[11], 12);
    R32(a2, b2, c2, d2, e2, m[0], 13);
    R31(e1, a1, b1, c1, d1, m[5], 7);
    R32(e2, a2, b2, c2, d2, m[4], 7);
    R31(d1, e1, a1, b1, c1, m[12], 5);
    R32(d2, e2, a2, b2, c2, m[13], 5);

    R41(c1, d1, e1, a1, b1, m[1], 11);
    R42(c2, d2, e2, a2, b2, m[8], 15);
    R41(b1, c1, d1, e1, a1, m[9], 12);
    R42(b2, c2, d2, e2, a2, m[6], 5);
    R41(a1, b1, c1, d1, e1, m[11], 14);
    R42(a2, b2, c2, d2, e2, m[4], 8);
    R41(e1, a1, b1, c1, d1, m[10], 15);
    R42(e2, a2, b2, c2, d2, m[1], 11);
    R41(d1, e1, a1, b1, c1, m[0], 14);
    R42(d2, e2, a2, b2, c2, m[3], 14);
    R41(c1, d1, e1, a1, b1, m[8], 15);
    R42(c2, d2, e2, a2, b2, m[11], 14);
    R41(b1, c1, d1, e1, a1, m[12], 9);
    R42(b2, c2, d2, e2, a2, m[15], 6);
    R41(a1, b1, c1, d1, e1, m[4], 8);
    R42(a2, b2, c2, d2, e2, m[0], 14);
    R41(e1, a1, b1, c1, d1, m[13], 9);
    R42(e2, a2, b2, c2, d2, m[5], 6);
    R41(d1, e1, a1, b1, c1, m[3], 14);
    R42(d2, e2, a2, b2, c2, m[12], 9);
    R41(c1, d1, e1, a1, b1, m[7], 5);
    R42(c2, d2, e2, a2, b2, m[2], 12);
    R41(b1, c1, d1, e1, a1, m[15], 6);
    R42(b2, c2, d2, e2, a2, m[13], 9);
    R41(a1, b1, c1, d1, e1, m[14], 8);
    R42(a2, b2, c2, d2, e2, m[9], 12);
    R41(e1, a1, b1, c1, d1, m[5], 6);
    R42(e2, a2, b2, c2, d2, m[7], 5);
    R41(d1, e1, a1, b1, c1, m[6], 5);
    R42(d2, e2, a2, b2, c2, m[10], 15);
    R41(c1, d1, e1, a1, b1, m[2], 12);
    R42(c2, d2, e2, a2, b2, m[14], 8);

    R51(b1, c1, d1, e1, a1, m[4], 9);
    R52(b2, c2, d2, e2, a2, m[12], 8);
    R51(a1, b1, c1, d1, e1, m[0], 15);
    R52(a2, b2, c2, d2, e2, m[15], 5);
    R51(e1, a1, b1, c1, d1, m[5], 5);
    R52(e2, a2, b2, c2, d2, m[10], 12);
    R51(d1, e1, a1, b1, c1, m[9], 11);
    R52(d2, e2, a2, b2, c2, m[4], 9);
    R51(c1, d1, e1, a1, b1, m[7], 6);
    R52(c2, d2, e2, a2, b2, m[1], 12);
    R51(b1, c1, d1, e1, a1, m[12], 8);
    R52(b2, c2, d2, e2, a2, m[5], 5);
    R51(a1, b1, c1, d1, e1, m[2], 13);
    R52(a2, b2, c2, d2, e2, m[8], 14);
    R51(e1, a1, b1, c1, d1, m[10], 12);
    R52(e2, a2, b2, c2, d2, m[7], 6);
    R51(d1, e1, a1, b1, c1, m[14], 5);
    R52(d2, e2, a2, b2, c2, m[6], 8);
    R51(c1, d1, e1, a1, b1, m[1], 12);
    R52(c2, d2, e2, a2, b2, m[2], 13);
    R51(b1, c1, d1, e1, a1, m[3], 13);
    R52(b2, c2, d2, e2, a2, m[13], 6);
    R51(a1, b1, c1, d1, e1, m[8], 14);
    R52(a2, b2, c2, d2, e2, m[14], 5);
    R51(e1, a1, b1, c1, d1, m[11], 11);
    R52(e2, a2, b2, c2, d2, m[0], 15);
    R51(d1, e1, a1, b1, c1, m[6], 8);
    R52(d2, e2, a2, b2, c2, m[3], 13);
    R51(c1, d1, e1, a1, b1, m[15], 5);
    R52(c2, d2, e2, a2, b2, m[9], 11);
    R51(b1, c1, d1, e1, a1, m[13], 6);
    R52(b2, c2, d2, e2, a2, m[11], 11);

    uint32_t* out = reinterpret_cast<uint32_t*>(outHash);
    out[0] = 0xEFCDAB89u + c1 + d2;
    out[1] = 0x98BADCFEu + d1 + e2;
    out[2] = 0x10325476u + e1 + a2;
    out[3] = 0xC3D2E1F0u + a1 + b2;
    out[4] = 0x67452301u + b1 + c2;
}