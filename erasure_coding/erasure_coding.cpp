#include "GF.h"
#include <bitset>
#include <xmmintrin.h>
#include <memory.h>
#include <immintrin.h>

#ifdef _WINDOWS
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

template<size_t I>
static bool check_indexes(int indexCount, const int(*indexes)[2]) noexcept {
    std::bitset<256> bitmap;
    for (int i = 0; i < indexCount; i++) {
        if (bitmap[indexes[i][I]]) return false;
        bitmap[indexes[i][I]] = true;
    }
    return true;
}

// 矩阵求逆
static void mat_inverse(GF* mat, int stride) noexcept {
    GF invMat[stride * stride];
    for (int i = 0; i < stride; i++) {
        invMat[i * stride + i] = 1;
    }

    for (int i = 0; i < stride; i++) {
        for (int r = i + 1; r < stride; r++) {
            GF scale = mat[r * stride + i] / mat[i * stride + i];
            if (scale == 0) continue;
            mat[r * stride + i] = 0;
            for (int c = i + 1; c < stride; c++) {
                mat[r * stride + c] += mat[i * stride + c] * scale;
            }
            for (int c = 0; c < stride; c++) {
                invMat[r * stride + c] += invMat[i * stride + c] * scale;
            }
        }
    }

    for (int i = stride - 1; i >= 0; i--) {
        for (int r = i - 1; r >= 0; r--) {
            GF scale = mat[r * stride + i] / mat[i * stride + i];
            if (scale == 0) continue;
            for (int c = i - 1; c >= 0; c--) {
                mat[r * stride + c] += mat[i * stride + c] * scale;
            }
            for (int c = stride - 1; c >= 0; c--) {
                invMat[r * stride + c] += invMat[i * stride + c] * scale;
            }
        }
    }

    for (int i = 0; i < stride; i++) {
        GF scale = mat[i * stride + i].inverse();
        if (scale == 1) continue;
        for (int j = 0; j < stride; j++) invMat[i * stride + j] *= scale;
    }

    memcpy(mat, invMat, stride * stride * sizeof(GF));
}

extern "C" {
    EXPORT int ec_encode(const u8* __restrict data, int dataBytes, u8* __restrict erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride) {
        if (dataStride <= 0 || dataStride >= 255) return 1;
        if (erasureCodeStride <= 0 || erasureCodeStride >= 255) return 2;
        if (dataBytes % dataStride != 0) return 3;
        if (erasureCodeBytes % erasureCodeStride != 0) return 4;

        int dataRows = dataBytes / dataStride;
        int ecRows = erasureCodeBytes / erasureCodeStride;
        if (dataRows != ecRows) return 5;

        GF const* multipliers[dataStride * erasureCodeStride];
        for (int r = 0, i = 0; r < erasureCodeStride; r++) {
            GF a = GF::from_exp(r);
            for (int c = 0; c < dataStride; c++, i++) {
                multipliers[i] = a.pow(c).multiplier();
                for (int offs = 0; offs < 256; offs += 64) {
                    _mm_prefetch(reinterpret_cast<const char*>(multipliers[i] + offs), _MM_HINT_NTA);
                }
            }
        }

        for (int r = 0, dataIndex = 0, ecIndex = 0; r < dataRows; r++, dataIndex += dataStride) {
            GF const* const* multiplier = multipliers;
            for (int ecSubIndex = 0; ecSubIndex < erasureCodeStride; ecSubIndex++, multiplier += dataStride) {
                GF s = 0;
                for (int i = 0; i < dataStride; i++) s += multiplier[i][data[dataIndex + i]];
                erasureCode[ecIndex++] = s;
            }
        }

        return 0;
    }

    EXPORT int ec_decode(u8* __restrict dataWithEC, int dataWithECBytes, int stride, const int(*__restrict indexes)[2], int indexCount) {
        if (stride <= 0 || stride >= 255) return 1;
        if (indexCount <= 0 || indexCount >= 255) return 2;
        if (dataWithECBytes % stride != 0) return 3;

        for (int i = 0; i < indexCount; i++) {
            if (indexes[i][0] < 0 || indexes[i][0] >= stride) return 4;
            if (indexes[i][1] < 0 || indexes[i][1] >= 254) return 4;
        }

        if (!check_indexes<0>(indexCount, indexes) || !check_indexes<1>(indexCount, indexes)) return 4;

        GF mat[stride * stride];
        for (int i = 0; i < stride; i++) {
            mat[i * stride + i] = 1;
        }

        for (int i = 0; i < indexCount; i++) {
            GF* row = mat + indexes[i][0] * stride;
            GF a = GF::from_exp(indexes[i][1]);
            for (int c = 0; c < stride; c++) {
                row[c] = a.pow(c);
            }
        }

        mat_inverse(mat, stride);

        GF const* multipliers[stride * indexCount];
        for (int r = 0; r < indexCount; r++) {
            for (int c = 0; c < stride; c++) {
                multipliers[r * stride + c] = mat[indexes[r][0] * stride + c].multiplier();
                for (int offs = 0; offs < 256; offs += 64) {
                    _mm_prefetch(reinterpret_cast<const char*>(multipliers[r * stride + c] + offs), _MM_HINT_NTA);
                }
            }
        }

        GF tmpResult[indexCount];
        int rows = dataWithECBytes / stride;
        for (int r = 0, dataIndex = 0; r < rows; r++, dataIndex += stride) {
            GF const* const* multiplier = multipliers;
            for (int i = 0; i < indexCount; i++, multiplier += stride) {
                GF s = 0;
                for (int c = 0; c < stride; c++) s += multiplier[c][dataWithEC[dataIndex + c]];
                tmpResult[i] = s;
            }
            for (int i = 0; i < indexCount; i++) {
                dataWithEC[dataIndex + indexes[i][0]] = tmpResult[i];
            }
        }

        return 0;
    }



    EXPORT int ec_encode_ssse3(const u8* __restrict data, int dataBytes, u8* __restrict erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride) {
        if (dataStride <= 0 || dataStride >= 255) return 1;
        if (erasureCodeStride <= 0 || erasureCodeStride >= 255) return 2;
        if (dataBytes % dataStride != 0) return 3;
        if (erasureCodeBytes % erasureCodeStride != 0) return 4;

        int dataRows = dataBytes / dataStride;
        int ecRows = erasureCodeBytes / erasureCodeStride;
        if (dataRows != ecRows) return 5;

        constexpr int N = sizeof(__m128i);

        __m128i table[dataStride * erasureCodeStride][2];
        for (int r = 0, i = 0; r < erasureCodeStride; r++) {
            GF a = GF::from_exp(r);
            for (int c = 0; c < dataStride; c++, i++) {
                GF t = a.pow(c);
                for (int n = 0; n < N; n++) {
                    reinterpret_cast<u8*>(&table[i][0])[n] = t * GF(n);
                    reinterpret_cast<u8*>(&table[i][1])[n] = t * GF(n << 4);
                }
            }
        }

        __m128i tempLine[dataStride];
        const __m128i mask = _mm_set1_epi8(0x0f);
        int r = 0, dataIndex = 0, ecIndex = 0;
        for (; r <= dataRows - N; r += N, ecIndex += erasureCodeStride * N) {
            for (int j = 0; j < N; j++) {
                for (int i = 0; i < dataStride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = data[dataIndex++];
                }
            }

            const __m128i(*pTable)[2] = table;
            for (int ecSubIndex = 0; ecSubIndex < erasureCodeStride; ecSubIndex++, pTable += dataStride) {
                __m128i s = _mm_set_epi32(0, 0, 0, 0);
                for (int i = 0; i < dataStride; i++) {
                    __m128i l = _mm_shuffle_epi8(pTable[i][0], _mm_and_si128(tempLine[i], mask));
                    __m128i h = _mm_shuffle_epi8(pTable[i][1], _mm_and_si128(_mm_srli_epi64(tempLine[i], 4), mask));
                    s = _mm_xor_si128(s, _mm_xor_si128(l, h));
                }

                u8* ecLine = erasureCode + ecIndex;
                for (int n = 0; n < N; n++, ecLine += erasureCodeStride) {
                    ecLine[ecSubIndex] = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }
        if (r < dataRows) {
            for (int j = 0; j < dataRows - r; j++) {
                for (int i = 0; i < dataStride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = data[dataIndex++];
                }
            }
            const __m128i(*pTable)[2] = table;
            for (int ecSubIndex = 0; ecSubIndex < erasureCodeStride; ecSubIndex++, pTable += dataStride) {
                __m128i s = _mm_setzero_si128();
                for (int i = 0; i < dataStride; i++) {
                    __m128i l = _mm_shuffle_epi8(pTable[i][0], _mm_and_si128(tempLine[i], mask));
                    __m128i h = _mm_shuffle_epi8(pTable[i][1], _mm_and_si128(_mm_srli_epi64(tempLine[i], 4), mask));
                    s = _mm_xor_si128(s, _mm_xor_si128(l, h));
                }

                u8* ecLine = erasureCode + ecIndex;
                for (int n = 0; n < dataRows - r; n++, ecLine += erasureCodeStride) {
                    ecLine[ecSubIndex] = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }

        return 0;
    }

    EXPORT int ec_decode_ssse3(u8* __restrict dataWithEC, int dataWithECBytes, int stride, const int(*__restrict indexes)[2], int indexCount) {
        if (stride <= 0 || stride >= 255) return 1;
        if (indexCount <= 0 || indexCount >= 255) return 2;
        if (dataWithECBytes % stride != 0) return 3;

        for (int i = 0; i < indexCount; i++) {
            if (indexes[i][0] < 0 || indexes[i][0] >= stride) return 4;
            if (indexes[i][1] < 0 || indexes[i][1] >= 254) return 4;
        }

        if (!check_indexes<0>(indexCount, indexes) || !check_indexes<1>(indexCount, indexes)) return 4;

        GF mat[stride * stride];
        for (int i = 0; i < stride; i++) {
            mat[i * stride + i] = 1;
        }

        for (int i = 0; i < indexCount; i++) {
            GF* row = mat + indexes[i][0] * stride;
            GF a = GF::from_exp(indexes[i][1]);
            for (int c = 0; c < stride; c++) {
                row[c] = a.pow(c);
            }
        }

        mat_inverse(mat, stride);

        constexpr int N = sizeof(__m128i);

        __m128i table[stride * indexCount][2];
        for (int r = 0, i = 0; r < indexCount; r++) {
            for (int c = 0; c < stride; c++, i++) {
                for (int n = 0; n < N; n++) {
                    GF a = mat[indexes[r][0] * stride + c];
                    reinterpret_cast<u8*>(&table[i][0])[n] = a * GF(n);
                    reinterpret_cast<u8*>(&table[i][1])[n] = a * GF(n << 4);
                }
            }
        }

        __m128i tempLine[stride];
        const __m128i mask = _mm_set1_epi8(0x0f);
        int rows = dataWithECBytes / stride;
        int r = 0, dataIndex = 0;
        for (; r <= rows - N; r += N) {
            for (int j = 0; j < N; j++) {
                for (int i = 0; i < stride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = dataWithEC[dataIndex++];
                }
            }

            const __m128i(*pTable)[2] = table;
            for (int i = 0; i < indexCount; i++, pTable += stride) {
                __m128i s = _mm_setzero_si128();
                for (int c = 0; c < stride; c++) {
                    __m128i l = _mm_shuffle_epi8(pTable[c][0], _mm_and_si128(tempLine[c], mask));
                    __m128i h = _mm_shuffle_epi8(pTable[c][1], _mm_and_si128(_mm_srli_epi64(tempLine[c], 4), mask));
                    s = _mm_xor_si128(s, _mm_xor_si128(l, h));
                }

                u8* ecLine = dataWithEC + r * stride + indexes[i][0];
                for (int n = 0; n < N; n++, ecLine += stride) {
                    *ecLine = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }
        if (r < rows) {
            for (int j = 0; j < rows - r; j++) {
                for (int i = 0; i < stride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = dataWithEC[dataIndex++];
                }
            }

            const __m128i(*pTable)[2] = table;
            for (int i = 0; i < indexCount; i++, pTable += stride) {
                __m128i s = _mm_set_epi32(0, 0, 0, 0);
                for (int c = 0; c < stride; c++) {
                    __m128i l = _mm_shuffle_epi8(pTable[c][0], _mm_and_si128(tempLine[c], mask));
                    __m128i h = _mm_shuffle_epi8(pTable[c][1], _mm_and_si128(_mm_srli_epi64(tempLine[c], 4), mask));
                    s = _mm_xor_si128(s, _mm_xor_si128(l, h));
                }

                u8* ecLine = dataWithEC + r * stride + indexes[i][0];
                for (int n = 0; n < rows - r; n++, ecLine += stride) {
                    *ecLine = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }

        return 0;
    }



    EXPORT int ec_encode_avx2(const u8* __restrict data, int dataBytes, u8* __restrict erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride) {
        if (dataStride <= 0 || dataStride >= 255) return 1;
        if (erasureCodeStride <= 0 || erasureCodeStride >= 255) return 2;
        if (dataBytes % dataStride != 0) return 3;
        if (erasureCodeBytes % erasureCodeStride != 0) return 4;

        int dataRows = dataBytes / dataStride;
        int ecRows = erasureCodeBytes / erasureCodeStride;
        if (dataRows != ecRows) return 5;

        constexpr int N = sizeof(__m256i);

        __m256i table[dataStride * erasureCodeStride][2];
        for (int r = 0, i = 0; r < erasureCodeStride; r++) {
            GF a = GF::from_exp(r);
            for (int c = 0; c < dataStride; c++, i++) {
                GF t = a.pow(c);
                for (int n = 0; n < 16; n++) {
                    reinterpret_cast<u8*>(&table[i][0])[n] = reinterpret_cast<u8*>(&table[i][0])[n + 16] = t * GF(n);
                    reinterpret_cast<u8*>(&table[i][1])[n] = reinterpret_cast<u8*>(&table[i][1])[n + 16] = t * GF(n << 4);
                }
            }
        }

        __m256i tempLine[dataStride];
        const __m256i mask = _mm256_set1_epi8(0x0f);
        int r = 0, dataIndex = 0, ecIndex = 0;
        for (; r <= dataRows - N; r += N, ecIndex += erasureCodeStride * N) {
            for (int j = 0; j < N; j++) {
                for (int i = 0; i < dataStride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = data[dataIndex++];
                }
            }

            const __m256i(*pTable)[2] = table;
            for (int ecSubIndex = 0; ecSubIndex < erasureCodeStride; ecSubIndex++, pTable += dataStride) {
                __m256i s = _mm256_setzero_si256();
                for (int i = 0; i < dataStride; i++) {
                    __m256i l = _mm256_shuffle_epi8(pTable[i][0], _mm256_and_si256(tempLine[i], mask));
                    __m256i h = _mm256_shuffle_epi8(pTable[i][1], _mm256_and_si256(_mm256_srli_epi64(tempLine[i], 4), mask));
                    s = _mm256_xor_si256(s, _mm256_xor_si256(l, h));
                }

                u8* ecLine = erasureCode + ecIndex;
                for (int n = 0; n < N; n++, ecLine += erasureCodeStride) {
                    ecLine[ecSubIndex] = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }
        if (r < dataRows) {
            for (int j = 0; j < dataRows - r; j++) {
                for (int i = 0; i < dataStride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = data[dataIndex++];
                }
            }
            const __m256i(*pTable)[2] = table;
            for (int ecSubIndex = 0; ecSubIndex < erasureCodeStride; ecSubIndex++, pTable += dataStride) {
                __m256i s = _mm256_setzero_si256();
                for (int i = 0; i < dataStride; i++) {
                    __m256i l = _mm256_shuffle_epi8(pTable[i][0], _mm256_and_si256(tempLine[i], mask));
                    __m256i h = _mm256_shuffle_epi8(pTable[i][1], _mm256_and_si256(_mm256_srli_epi64(tempLine[i], 4), mask));
                    s = _mm256_xor_si256(s, _mm256_xor_si256(l, h));
                }

                u8* ecLine = erasureCode + ecIndex;
                for (int n = 0; n < dataRows - r; n++, ecLine += erasureCodeStride) {
                    ecLine[ecSubIndex] = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }

        return 0;
    }

    EXPORT int ec_decode_avx2(u8* __restrict dataWithEC, int dataWithECBytes, int stride, const int(*__restrict indexes)[2], int indexCount) {
        if (stride <= 0 || stride >= 255) return 1;
        if (indexCount <= 0 || indexCount >= 255) return 2;
        if (dataWithECBytes % stride != 0) return 3;

        for (int i = 0; i < indexCount; i++) {
            if (indexes[i][0] < 0 || indexes[i][0] >= stride) return 4;
            if (indexes[i][1] < 0 || indexes[i][1] >= 254) return 4;
        }

        if (!check_indexes<0>(indexCount, indexes) || !check_indexes<1>(indexCount, indexes)) return 4;

        GF mat[stride * stride];
        for (int i = 0; i < stride; i++) {
            mat[i * stride + i] = 1;
        }

        for (int i = 0; i < indexCount; i++) {
            GF* row = mat + indexes[i][0] * stride;
            GF a = GF::from_exp(indexes[i][1]);
            for (int c = 0; c < stride; c++) {
                row[c] = a.pow(c);
            }
        }

        mat_inverse(mat, stride);

        constexpr int N = sizeof(__m256i);

        __m256i table[stride * indexCount][2];
        for (int r = 0, i = 0; r < indexCount; r++) {
            for (int c = 0; c < stride; c++, i++) {
                for (int n = 0; n < 16; n++) {
                    GF a = mat[indexes[r][0] * stride + c];
                    reinterpret_cast<u8*>(&table[i][0])[n] = reinterpret_cast<u8*>(&table[i][0])[n + 16] = a * GF(n);
                    reinterpret_cast<u8*>(&table[i][1])[n] = reinterpret_cast<u8*>(&table[i][1])[n + 16] = a * GF(n << 4);
                }
            }
        }

        __m256i tempLine[stride];
        const __m256i mask = _mm256_set1_epi8(0x0f);
        int rows = dataWithECBytes / stride;
        int r = 0, dataIndex = 0;
        for (; r <= rows - N; r += N) {
            for (int j = 0; j < N; j++) {
                for (int i = 0; i < stride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = dataWithEC[dataIndex++];
                }
            }

            const __m256i(*pTable)[2] = table;
            for (int i = 0; i < indexCount; i++, pTable += stride) {
                __m256i s = _mm256_setzero_si256();
                for (int c = 0; c < stride; c++) {
                    __m256i l = _mm256_shuffle_epi8(pTable[c][0], _mm256_and_si256(tempLine[c], mask));
                    __m256i h = _mm256_shuffle_epi8(pTable[c][1], _mm256_and_si256(_mm256_srli_epi64(tempLine[c], 4), mask));
                    s = _mm256_xor_si256(s, _mm256_xor_si256(l, h));
                }

                u8* ecLine = dataWithEC + r * stride + indexes[i][0];
                for (int n = 0; n < N; n++, ecLine += stride) {
                    *ecLine = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }
        if (r < rows) {
            for (int j = 0; j < rows - r; j++) {
                for (int i = 0; i < stride; i++) {
                    reinterpret_cast<u8*>(tempLine + i)[j] = dataWithEC[dataIndex++];
                }
            }

            const __m256i(*pTable)[2] = table;
            for (int i = 0; i < indexCount; i++, pTable += stride) {
                __m256i s = _mm256_setzero_si256();
                for (int c = 0; c < stride; c++) {
                    __m256i l = _mm256_shuffle_epi8(pTable[c][0], _mm256_and_si256(tempLine[c], mask));
                    __m256i h = _mm256_shuffle_epi8(pTable[c][1], _mm256_and_si256(_mm256_srli_epi64(tempLine[c], 4), mask));
                    s = _mm256_xor_si256(s, _mm256_xor_si256(l, h));
                }

                u8* ecLine = dataWithEC + r * stride + indexes[i][0];
                for (int n = 0; n < rows - r; n++, ecLine += stride) {
                    *ecLine = reinterpret_cast<const u8*>(&s)[n];
                }
            }
        }

        return 0;
    }
}