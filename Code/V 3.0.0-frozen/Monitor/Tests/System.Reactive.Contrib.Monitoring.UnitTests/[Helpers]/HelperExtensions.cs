#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Concurrency;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    /// <summary>
    /// Helper extensions
    /// </summary>
    public static class HelperExtensions
    {
        #region Delay

        /// <summary>
        /// Delays the specified scheduler.
        /// </summary>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static Task Delay(this IScheduler scheduler, TimeSpan duration)
        {
            var tcs = new TaskCompletionSource<Unit>();
            scheduler.Schedule(duration, () => tcs.SetResult(Unit.Default));
            return tcs.Task;
        }

        #endregion // Delay
    }
}