#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class BasicTests : TestsBase
    {
        #region ReturnTest

        [TestMethod]
        public void ReturnTest()
        {
            IObservable<string> xs;
            using (ScopeFactory.Create())
            {
                xs = Observable.Return("Hello Rx");
                xs = xs.Monitor("Return", 1);
                xs.Subscribe();
            }
            GC.KeepAlive(xs);
        }

        #endregion ReturnTest

        #region IgnoreElementsBasicTest

        [TestMethod]
        public void IgnoreElementsBasicTest()
        {
            IObservable<int> ys;
            using (ScopeFactory.Create(2))
            {
                var xs = Observable.Range(0, 100).Take(10);
                xs = xs.Monitor("Range", 1);
                ys = xs.IgnoreElements();
                ys = ys.Monitor("Ignore", 2);
                ys.Subscribe();
            }
            GC.KeepAlive(ys);
        }

        #endregion IgnoreElementsBasicTest

        #region StartWithTest

        [TestMethod]
        public void StartWithTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
            xs = xs.Monitor("Interval", 1);
            var ys = xs.StartWith(-1, -2, -3);
            ys = ys.Monitor("StartWith", 2);
            ys.Wait();
        }

        #endregion StartWithTest

        #region SampleTest

        [TestMethod]
        public void SampleTest()
        {
            var sw = Stopwatch.StartNew();

            var xs = Observable.Interval(TimeSpan.FromMilliseconds(1), NewThreadScheduler.Default)
                .Take(4000);

            xs = xs.Monitor("Source", 1);
            var ys = xs.Sample(TimeSpan.FromSeconds(0.5));
            ys = ys.Monitor("Sample", 2);
            ys.Wait();

            sw.Stop();
            Trace.WriteLine("### " + sw.ElapsedMilliseconds);
        }

        #endregion SampleTest

        #region SampleOnGenerateTest

        [TestMethod]
        public void SampleOnGenerateTest()
        {
            var xs = Observable.Generate(0, i => i < 6, i => i + 1, i => i, i => TimeSpan.FromSeconds(i));
            xs = xs.Monitor("A", 1);
            var ys = xs.Sample(TimeSpan.FromSeconds(3));
            ys = ys.Monitor("Sample", 2);
            ys.Wait();
        }

        #endregion SampleOnGenerateTest

        #region DistinctUntilChangedTest

        [TestMethod]
        public void DistinctUntilChangedTest()
        {
            Random rnd = new Random(Environment.TickCount);
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(20)
                .Select(_ => rnd.Next(2));
            xs = xs.Monitor("A", 1);
            var ys = xs.DistinctUntilChanged();
            ys = ys.Monitor("DistinctUntilChanged", 2);
            ys.Wait();
        }

        #endregion DistinctUntilChangedTest

        #region ContainsTest

        [TestMethod]
        public void ContainsTest()
        {
            Random rnd = new Random(Environment.TickCount);
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(15)
                .Select(_ => rnd.Next(7))
                .Monitor("Source", 1)
                .Publish();
            using (ScopeFactory.Create(2))
            {
                var ys = xs.Contains(2);
                ys = ys.Monitor("Contains", 2);
                ys.Subscribe();
                xs.Connect();
            }
            GC.KeepAlive(xs);
        }

        #endregion ContainsTest
    }
}