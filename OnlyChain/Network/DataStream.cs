using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Network {
    public class DataStream : Stream {
        private readonly NetworkStream stream;
        private int position = 0;
        private readonly int length;

        public DataStream(NetworkStream baseStream, int length) {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get => position; set => throw new InvalidOperationException(); }

        public override void Flush() {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            ValidateBufferArguments(buffer, offset, count);

            int length = Math.Min(count, this.length - position);
            if (length == 0) return 0;

            int readBytes = stream.Read(buffer, offset, length);
            position += readBytes;
            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value) {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new InvalidOperationException();
        }

        private static void ValidateBufferArguments(byte[] buffer, int offset, int size) {
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            if ((uint)offset > (uint)buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)size > (uint)(buffer.Length - offset)) {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
        }
    }
}
