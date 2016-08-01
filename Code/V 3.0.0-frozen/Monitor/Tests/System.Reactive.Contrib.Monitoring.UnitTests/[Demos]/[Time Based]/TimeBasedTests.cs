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

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class TimeBasedTests : TestsBase
    {
        #region SingleIntervalTest

        [TestMethod]
        public void SingleIntervalTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
            xs = xs.Monitor("Interval 1 second", 1);
            //xs.Subscribe();

            xs.Wait();
            GC.KeepAlive(xs);
        }

        #endregion SingleIntervalTest

        #region IntervalTest

        [TestMethod]
        public void IntervalTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
            xs = xs.Monitor("Interval 1 second", 1);
            var ys = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
            ys = ys.Monitor("Interval 0.5 second", 2);
            xs.Subscribe();

            ys.Wait();
            GC.KeepAlive(xs);
        }

        #endregion IntervalTest

        #region IntervalVsTimerTest

        [TestMethod]
        public void IntervalVsTimerTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
            xs = xs.Monitor("Interval 1 second", 1);
            var ys = Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.5)).Take(10);
            ys = ys.Monitor("Timer 3 second follow by 0.5 second", 2);
            xs.Subscribe();

            ys.Wait();
            GC.KeepAlive(xs);
        }

        #endregion IntervalVsTimerTest

        #region Interval_WithKeywords_Test

        [TestMethod]
        public void Interval_WithKeywords_Test()
        {
            var xs1 = Observable.Interval(TimeSpan.FromSeconds(3)).Take(3);
            xs1 = xs1.Monitor("Interval 3 second", 1,
                                "Merge scenario", "intervals");
            var xs2 = Observable.Interval(TimeSpan.FromSeconds(2.5)).Take(4);
            xs2 = xs2.Monitor("Interval 2.5 second", 2,
                                "Merge scenario", "intervals");

            var ms = Observable.Merge(xs1, xs2);
            ms = ms.Monitor("Merged", 3, "Merge scenario");

            var xs3 = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
            xs3 = xs3.Monitor("Interval 1 second", 4, "intervals");

            xs3.Subscribe();
            ms.Wait();

            GC.KeepAlive(xs3);
        }

        #endregion Interval_WithKeywords_Test

        #region DelaySingleTest

        [TestMethod]
        public void DelaySingleTest()
        {
            var xs = Observable.Return("Hello Rx");
            xs = xs.Monitor("Source", 1);
            var ds = xs.Delay(TimeSpan.FromSeconds(2));
            ds = ds.Monitor("Delayed", 2);

            ds.Wait();
        }

        #endregion DelaySingleTest

        #region DelayTest

        [TestMethod]
        public void DelayTest()
        {
            var rnd = new Random(Environment.TickCount);
            var xs = Observable.Generate(0, i => i < 10, i => i + 1, i => i, _ => TimeSpan.FromSeconds(rnd.Next(1, 3)));
            xs = xs.Monitor("Source", 1);
            var ds = xs.Delay(TimeSpan.FromSeconds(2));
            ds = ds.Monitor("Delayed", 2);

            ds.Wait();
        }

        #endregion DelayTest

        #region TimeIntervalTest

        [TestMethod]
        public void TimeIntervalTest()
        {
            var rnd = new Random(Environment.TickCount);
            var xs = Observable.Generate(0, i => i < 10, i => i + 1, i => i,
                _ => TimeSpan.FromSeconds(rnd.Next(1, 3)))
                .Timestamp()
                .Select(v => v.Timestamp);
            xs = xs.Monitor("Source", 1);
            var ds = xs.TimeInterval()
                .Select(v => v.Interval);
            ds = ds.Monitor("TimeInterval", 2);
            ds.Wait();
        }

        #endregion TimeIntervalTest

        #region ThrottleTest

        [TestMethod]
        public void ThrottleTest()
        {
            var rnd = new Random(Environment.TickCount);
            var xs = Observable.Generate(0, i => i < 10, i => i + 1, i => i,
                _ => TimeSpan.FromSeconds(rnd.NextDouble() * 3 + 0.5))
                .Timestamp()
                .Select(v => v.Timestamp);
            xs = xs.Monitor("Source", 1);
            var ds = xs.Throttle(TimeSpan.FromSeconds(2))
                .Timestamp()
                .Select(v => v.Timestamp);
            ds = ds.Monitor("Throttle", 2);
            ds.Wait();
        }

        #endregion ThrottleTest
    }
}