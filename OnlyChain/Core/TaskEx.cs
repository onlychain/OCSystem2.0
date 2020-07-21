using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public static class TaskEx {
        public static async Task WhenAll(this IEnumerable<Task> tasks, bool noThrow = true) {
            if (noThrow) {
                try { await Task.WhenAll(tasks); } catch { }
            } else {
                await Task.WhenAll(tasks);
            }
        }
    }
}
