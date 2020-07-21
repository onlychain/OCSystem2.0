using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security;

namespace OnlyChain.Coding {
    unsafe public static class ErasureCoding {
        private const string Dll = "erasure_coding";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_encode")]
        extern static int NativeEncode(byte* data, int dataBytes, byte* erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_decode")]
        extern static int NativeDecode(byte* dataWithEC, int dataWithECBytes, int stride, ErasureCodingIndex* indexes, int indexCount);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_encode_ssse3")]
        extern static int NativeEncodeSsse3(byte* data, int dataBytes, byte* erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_decode_ssse3")]
        extern static int NativeDecodeSsse3(byte* dataWithEC, int dataWithECBytes, int stride, ErasureCodingIndex* indexes, int indexCount);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_encode_avx2")]
        extern static int NativeEncodeAvx2(byte* data, int dataBytes, byte* erasureCode, int erasureCodeBytes, int dataStride, int erasureCodeStride);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Dll, EntryPoint = "ec_decode_avx2")]
        extern static int NativeDecodeAvx2(byte* dataWithEC, int dataWithECBytes, int stride, ErasureCodingIndex* indexes, int indexCount);

        /// <summary>
        /// 填充<paramref name="erasureCode"/>
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="erasureCode">纠错码</param>
        /// <param name="dataStride">数据跨度，大于0且小于255。<paramref name="data"/>长度必须是<paramref name="dataStride"/>的整数倍</param>
        /// <param name="erasureCodeStride">纠错码跨度，大于0且小于255。<paramref name="erasureCode"/>长度必须是<paramref name="erasureCodeStride"/>的整数倍</param>
        public static void Encode(ReadOnlySpan<byte> data, Span<byte> erasureCode, int dataStride, int erasureCodeStride) {
            fixed (byte* pData = data)
            fixed (byte* pEC = erasureCode) {
                int errorCode;
                if (Avx2.IsSupported) {
                    errorCode = NativeEncodeAvx2(pData, data.Length, pEC, erasureCode.Length, dataStride, erasureCodeStride);
                } else if (Ssse3.IsSupported) {
                    errorCode = NativeEncodeSsse3(pData, data.Length, pEC, erasureCode.Length, dataStride, erasureCodeStride);
                } else {
                    errorCode = NativeEncode(pData, data.Length, pEC, erasureCode.Length, dataStride, erasureCodeStride);
                }
                if (errorCode == 0) return;
                throw errorCode switch
                {
                    1 => new ArgumentOutOfRangeException(nameof(dataStride)),
                    2 => new ArgumentOutOfRangeException(nameof(erasureCodeStride)),
                    3 => new ArgumentException($"{nameof(data)}的字节数必须是{nameof(dataStride)}的整数倍"),
                    4 => new ArgumentException($"{nameof(erasureCode)}的字节数必须是{nameof(erasureCodeStride)}的整数倍"),
                    5 => new ArgumentException($"{nameof(data)}的行数必须和{nameof(erasureCode)}一致"),
                    _ => new BadImageFormatException("未知错误，可能是动态链接库版本不一致"),
                };
            }
        }

        /// <summary>
        /// 恢复<paramref name="dataWithEC"/>
        /// </summary>
        /// <param name="dataWithEC">数据与纠错码</param>
        /// <param name="stride">原始数据的跨度，大于0且小于255。<paramref name="dataWithEC"/>长度必须是<paramref name="stride"/>的整数倍</param>
        /// <param name="indexes">指定每个跨度缺失的数据索引以及代替的纠错码索引</param>
        public static void Decode(Span<byte> dataWithEC, int stride, ReadOnlySpan<ErasureCodingIndex> indexes) {
            fixed (byte* pDataWithEC = dataWithEC)
            fixed (ErasureCodingIndex* pIndexes = indexes) {
                int errorCode;
                if (Avx2.IsSupported) {
                    errorCode = NativeDecodeAvx2(pDataWithEC, dataWithEC.Length, stride, pIndexes, indexes.Length);
                } else if (Ssse3.IsSupported) {
                    errorCode = NativeDecodeSsse3(pDataWithEC, dataWithEC.Length, stride, pIndexes, indexes.Length);
                } else {
                    errorCode = NativeDecode(pDataWithEC, dataWithEC.Length, stride, pIndexes, indexes.Length);
                }
                if (errorCode == 0) return;
                throw errorCode switch
                {
                    1 => new ArgumentOutOfRangeException(nameof(stride)),
                    2 => new ArgumentOutOfRangeException(nameof(indexes), $"{nameof(indexes)}数量过多"),
                    3 => new ArgumentException($"{nameof(dataWithEC)}的字节数必须是{nameof(stride)}的整数倍"),
                    4 => new ArgumentException($"{nameof(indexes)}包含无效的索引", nameof(indexes)),
                    _ => new BadImageFormatException("未知错误，可能是动态链接库版本不一致"),
                };
            }
        }
    }
}
