using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    unsafe public sealed class NativeBuffer {
        public readonly byte* Data;
        public readonly int Length;

        public Span<byte> Span => new Span<byte>(Data, Length);

        public ReadOnlySpan<byte> ReadOnlySpan => new ReadOnlySpan<byte>(Data, Length);

        /// <summary>
        /// 拷贝Buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public NativeBuffer(byte* data, int length) {
            Data = (byte*)Marshal.AllocHGlobal(length);
            Length = length;

            new ReadOnlySpan<byte>(data, length).CopyTo(new Span<byte>(Data, Length));
        }


        ~NativeBuffer() {
            Marshal.FreeHGlobal((IntPtr)Data);
        }
    }
}
