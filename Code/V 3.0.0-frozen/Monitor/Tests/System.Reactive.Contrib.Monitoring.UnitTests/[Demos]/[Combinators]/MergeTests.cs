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
    public class MergeTests : TestsBase
    {
        #region SimpleTest

        [TestMethod]
        public void SimpleTest()
        {
            var xs1 = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
            xs1 = xs1.Monitor("Interval", 1);
            var xs2 = Observable.Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(2)).Take(5);
            xs2 = xs2.Monitor("Timer", 2);
            var ms = Observable.Merge(xs1, xs2);
            ms = ms.Monitor("Merge", 3);
            //ms1.Subscribe();
            ms.Wait();
        }

        #endregion SimpleTest

        #region IntervalTest

        [TestMethod]
        public void IntervalTest()
        {
            var xs1 = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
            xs1 = xs1.Monitor("Interval 1 second", 1, "Group Odd", "Intervals");
            var xs2 = Observable.Interval(TimeSpan.FromSeconds(2)).Take(5);
            xs2 = xs2.Monitor("Interval 2 second", 2, "Group Even", "Intervals");
            var xs3 = Observable.Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(3)).Take(3);
            xs3 = xs3.Monitor("Interval 3 second", 3, "Group Odd", "Intervals");
            var xs4 = Observable.Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(4)).Take(2);
            xs4 = xs4.Monitor("Interval 4 second", 4, "Group Even", "Intervals");
            var ms1 = Observable.Merge(xs1, xs3);
            ms1 = ms1.Monitor("Merge 1,3", 4, "Group Odd", "Merge");
            var ms2 = Observable.Merge(xs2, xs4);
            ms2 = ms2.Monitor("Merge 2,4", 5, "Group Even", "Merge");
            //ms1.Subscribe();
            ms2.Subscribe();

            ms1.Wait();
            GC.KeepAlive(ms2);
            Thread.Sleep(100);
        }

        #endregion IntervalTest
    }
}