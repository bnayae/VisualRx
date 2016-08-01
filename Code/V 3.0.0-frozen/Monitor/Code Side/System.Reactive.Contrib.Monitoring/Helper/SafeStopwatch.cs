#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// thread safe stopwatch
    /// </summary>
    public static class SafeStopwatch
    {
        private static DateTimeOffset _baseTime = DateTimeOffset.UtcNow;

        private static ThreadLocal<Tuple<Stopwatch, DateTimeOffset>> _stopwatch =
            new ThreadLocal<Tuple<Stopwatch, DateTimeOffset>>(() =>
                Tuple.Create(Stopwatch.StartNew(), DateTimeOffset.UtcNow));

        /// <summary>
        /// Get Elapse time
        /// </summary>
        /// <returns></returns>
        public static TimeSpan Elapsed()
        {
            Stopwatch sw = _stopwatch.Value.Item1;
            DateTimeOffset offset = _stopwatch.Value.Item2;
            TimeSpan duration = offset - _baseTime + sw.Elapsed;
            return duration;
        }
    }
}