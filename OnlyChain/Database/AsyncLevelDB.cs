#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OnlyChain.Database {
    internal sealed class AsyncLevelDB : IDisposable {
        readonly static int Concurrency = Environment.ProcessorCount;

        private readonly LevelDB db;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Channel<(ReadOnlyMemory<byte> Key, TaskCompletionSource<byte[]?>)> channel;

        public AsyncLevelDB(LevelDB db) {
            this.db = db;
            cancellationTokenSource = new CancellationTokenSource();
            channel = Channel.CreateUnbounded<(ReadOnlyMemory<byte> Key, TaskCompletionSource<byte[]?>)>();

            for (int i = 0; i < Concurrency; i++) {
                Loop();
            }
        }

        private async void Loop() {
            await foreach (var (key, tcs) in channel.Reader.ReadAllAsync(cancellationTokenSource.Token)) {
                try {
                    tcs.SetResult(db.Get(key.Span));
                } catch { }
            }
        }

        public async ValueTask<byte[]?> Get(ReadOnlyMemory<byte> key) {
            var tcs = new TaskCompletionSource<byte[]?>();
            await channel.Writer.WriteAsync((key, tcs));
            return await tcs.Task;
        }

        public async ValueTask<T[]?> Get<T>(ReadOnlyMemory<byte> key) where T : unmanaged {
            if (await Get(key) is byte[] value) {
                return MemoryMarshal.Cast<byte, T>(value).ToArray();
            }
            return null;
        }


        public void Dispose() {
            channel.Writer.Complete();
            cancellationTokenSource.Cancel();
            db.Dispose();
        }
    }
}
