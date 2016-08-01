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
    public class CombinatorsTests : TestsBase
    {
        #region RepeatSimpleTest

        [TestMethod]
        public void RepeatSimpleTest()
        {
            var xs = Observable.Repeat(1, 10);
            xs = xs.Monitor("Repeat", 1);

            xs.Wait();
            Thread.Sleep(100);
        }

        #endregion RepeatSimpleTest

        #region RepeatTest

        [TestMethod]
        public void RepeatTest()
        {
            var xs = Observable.Range(1, 3);
            xs = xs.Monitor("Source", 1);
            var ys = xs.Repeat(3);
            ys = ys.Monitor("Repeat", 2);

            ys.Wait();
            Thread.Sleep(100);
        }

        #endregion RepeatTest

        #region ConcatTest

        [TestMethod]
        public void ConcatTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(4);
            xs = xs.Monitor("Source", 1);
            var zs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Select(i => i + 10)
                .Take(3);
            zs = zs.Monitor("B", 2);
            var ys = xs.Concat(zs);
            ys = ys.Monitor("Concat", 3);

            ys.Wait(); ;
            Thread.Sleep(100);
        }

        #endregion ConcatTest
    }
}