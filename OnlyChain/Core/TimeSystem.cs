#nullable enable

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public static class TimeSystem {
        static readonly DateTime beginTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static readonly Stopwatch referenceTimer = Stopwatch.StartNew();
        static double referenceOffsetSeconds;
        static AsyncTaskMethodBuilder taskCompletionSource;
        static Timer? refreshTask;

        [DebuggerDisplay("{ServerTime}, {DelayTime}, {RefTime}")]
        sealed class MeasuringTimes {
            public ulong ServerTime;
            public TimeSpan DelayTime;
            public TimeSpan RefTime;
        }

        /// <summary>
        /// 估算目标NTP服务器时间与本地计时器(<see cref="referenceTimer"/>)的秒数差距。
        /// </summary>
        /// <remarks>
        /// <see cref="beginTime"/> + <see cref="referenceTimer"/> + 该返回值 = 估算的UTC时间
        /// </remarks>
        /// <param name="host"></param>
        /// <returns></returns>
        static double? GetReferenceOffsetSeconds(string host) {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint endPoint = new DnsEndPoint(host, 123);
            try { socket.Connect(endPoint); } catch { return null; }
            socket.ReceiveTimeout = 500;
            var delayTimer = new Stopwatch();

            const int N = 10;
            var result = new MeasuringTimes[N];

            for (int i = 0; i < N; i++) {
                try {
                    Span<byte> buffer = stackalloc byte[48];
                    buffer[0] = 0x1b;

                    delayTimer.Restart();
                    socket.Send(buffer);
                    socket.Receive(buffer);
                    delayTimer.Stop();

                    TimeSpan refTime = referenceTimer.Elapsed;
                    ulong serverTime1 = BinaryPrimitives.ReadUInt64BigEndian(buffer[32..]);
                    ulong serverTime2 = BinaryPrimitives.ReadUInt64BigEndian(buffer[40..]);
                    double diff = unchecked((long)(serverTime2 - serverTime1)) / (double)(1L << 32);
                    TimeSpan delay = delayTimer.Elapsed.Subtract(TimeSpan.FromSeconds(diff));
                    result[i] = new MeasuringTimes {
                        ServerTime = serverTime2,
                        DelayTime = delay,
                        RefTime = refTime
                    };
                } catch (SocketException) { }
            }

            result = result.OfType<MeasuringTimes>().ToArray(); // 去掉null元素
            if (result.Length == 0) return null;
            TimeSpan avgDelay = new TimeSpan((long)result.Average(t => t.DelayTime.Ticks));
            var validResult = result
                .Where(t => t.DelayTime <= avgDelay)
                .Select(t => (Time: t.ServerTime / (double)(1L << 32) + t.DelayTime.TotalSeconds / 2, RefTime: t.RefTime.TotalSeconds))
                .ToArray();
            // 目标函数: y = x + t0
            // y: 当前时间与1900-1-1 00:00:00距离的秒数
            // x: 本地计时器经历的秒数（即referenceTimer）
            // t0: 待求
            // 通过最小二乘法求得t0。
            return validResult.Average(t => t.Time - t.RefTime);
        }

        static readonly string[] NTPServers = {
            "ntp.ntsc.ac.cn", // 中科院
            "cn.ntp.org.cn", // 中国授时
            "time1.aliyun.com", // 阿里云
            "time2.aliyun.com",
            "time3.aliyun.com",
            "time4.aliyun.com",
            "time5.aliyun.com",
            "time6.aliyun.com",
            "time7.aliyun.com",
            "time1.cloud.tencent.com", // 腾讯云
            "time2.cloud.tencent.com",
            "time3.cloud.tencent.com",
            "time4.cloud.tencent.com",
            "time5.cloud.tencent.com",
            "pool.ntp.org", // 国际通用
            "cn.pool.ntp.org",
            "time.nist.gov",
            "time.windows.com", // 微软
            "time.google.com", // 谷歌
            "time1.apple.com", // 苹果
            "time2.apple.com",
            "time3.apple.com",
            "time4.apple.com",
            "time5.apple.com",
            "time6.apple.com",
            "time7.apple.com",
        };

        static double GetReferenceOffsetSeconds() {
            var tasks = new List<Task<double?>>();
            foreach (var ntp in NTPServers) {
                tasks.Add(Task.Run(() => GetReferenceOffsetSeconds(ntp)));
            }
            Task.WaitAll(tasks.ToArray());
            var result = tasks.Select(t => t.Result).OfType<double>().ToList();

            while (true) {
                double avg = result.Average();
                double stdDevn = Math.Sqrt(result.Average(d => Math.Pow(d - avg, 2)));
                if (stdDevn > 0.002) {
                    int removeIndex = 0;
                    double max = Math.Abs(result[0] - avg);
                    for (int i = 1; i < result.Count; i++) {
                        double diff = Math.Abs(result[i] - avg);
                        if (diff > max) {
                            removeIndex = i;
                            max = diff;
                        }
                    }
                    result.RemoveAt(removeIndex);
                } else {
                    return avg;
                }
            }
        }

        public static Task Init() {
            lock (referenceTimer) {
                if (refreshTask is null) {
                    taskCompletionSource = AsyncTaskMethodBuilder.Create();

                    refreshTask = new Timer(delegate {
                        referenceOffsetSeconds = GetReferenceOffsetSeconds();
                    }, null, TimeSpan.FromHours(6), TimeSpan.FromHours(6));

                    Task.Run(delegate {
                        referenceOffsetSeconds = GetReferenceOffsetSeconds();
                        taskCompletionSource.SetResult();
                    });
                }

                return taskCompletionSource.Task;
            }
        }

        public static DateTime Now => beginTime.AddSeconds(referenceTimer.Elapsed.TotalSeconds + referenceOffsetSeconds);
    }
}
