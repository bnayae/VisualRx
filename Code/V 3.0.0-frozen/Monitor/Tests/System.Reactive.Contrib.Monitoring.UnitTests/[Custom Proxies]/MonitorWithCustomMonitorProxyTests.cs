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
    [TestClass]
    public class MonitorWithCustomMonitorProxyTests
    {
        #region TestInitialize

        [TestInitialize()]
        public void TestInitialize()
        {
            VisualRxSettings.ClearFilters();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Thread.Sleep(1);
        }

        #endregion TestInitialize

        #region ProxyHandleAllMonitordMessagesTest

        [TestMethod]
        public void ProxyHandleAllMonitordMessagesTest()
        {
            const int SIZE = 1000;
            //arrange
            var proxy = new CustomCountdownMonitorProxy("test", SIZE + 1 /* + 1 = On Completed */);
            var xs = Observable.Range(1, SIZE)
                .Monitor("Source", 0);

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(proxy);
            info.Wait();

            // act
            xs.Subscribe();
            //VisualRxSettings.WaitForProxiesCompletion(
            //    1, // number of proxies * number of streams
            //    TimeSpan.FromSeconds(0.5));
            Assert.IsTrue(proxy.Wait(), "Wait");

            // validate
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxy.Data.Count());

            GC.KeepAlive(xs);
        }

        #endregion ProxyHandleAllMonitordMessagesTest

        #region WaitForProxiesCompletionTest

        [TestMethod]
        public void WaitForProxiesCompletionTest()
        {
            const int SIZE = 10;
            //arrange
            var proxy = new CustomCountdownMonitorProxy("test", SIZE + 1 /* + 1 = On Completed */);
            var xs = Observable.Range(1, SIZE)
                .Monitor("A", 0);

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(proxy);
            info.Wait();

            // act
            xs.Delay(TimeSpan.FromMilliseconds(100)).Subscribe();
            //Assert.IsTrue(VisualRxSettings.WaitForProxiesCompletion(
            //    1 /* (streams) */,
            //    TimeSpan.FromSeconds(1)), "WaitForProxiesCompletion");
            Assert.IsTrue(proxy.Wait(), "Wait");

            // validate
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxy.Data.Count());

            GC.KeepAlive(xs);
        }

        #endregion WaitForProxiesCompletionTest

        #region ProxyFilterMessagesByKeywordsTest

        [TestMethod]
        public void ProxyFilterMessagesByKeywordsTest()
        {
            const int SIZE = 10;// 00;
            //arrange
            var proxyX = new CustomCountdownMonitorProxy("testX", SIZE + 1 /* + 1 = On Completed */);
            var proxyY = new CustomCountdownMonitorProxy("testY", SIZE + 1 /* + 1 = On Completed */);
            var proxyAll = new CustomCountdownMonitorProxy("testAll", (SIZE * 3) + 3 /* + 1  = On Completed */);

            var xs = Observable.Range(1, SIZE)
                .Monitor("X", 0, "Category1", "Category2");
            var ys = Observable.Range(1, SIZE)
                .Monitor("Y", 1, "Category1", "Category3");
            var zs = Observable.Range(1, SIZE)
                .Monitor("Z", 2);

            Task<VisualRxInitResult> info =
                VisualRxSettings.Initialize(proxyX, proxyY, proxyAll);

            VisualRxSettings.AddFilter((marble, proxyKind) =>
                {
                    if (proxyKind == "testX")
                        return marble.Keywords.Contains("Category2");
                    if (proxyKind == "testY")
                        return marble.Keywords.Contains("Category3");
                    return true;
                });
            info.Wait();

            // act
            xs.Subscribe();
            ys.Subscribe();
            zs.Subscribe();
            //Assert.IsTrue(VisualRxSettings.WaitForProxiesCompletion(
            //    3  /* (streams) */,
            //    TimeSpan.FromSeconds(1)), "WaitForProxiesCompletion");
            Assert.IsTrue(proxyX.Wait(), "Wait");
            Assert.IsTrue(proxyY.Wait(), "Wait");
            Assert.IsTrue(proxyAll.Wait(), "Wait");

            // validate
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxyX.Data.Count(), "proxyX");
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxyY.Data.Count(), "proxyY");
            Assert.AreEqual((SIZE * 3) + 3 /* + 1  = On Completed */, proxyAll.Data.Count(), "proxyAll");

            Assert.IsFalse(proxyX.Data.Any(marble =>
                !marble.Keywords.Contains("Category2")));
            Assert.IsFalse(proxyY.Data.Any(marble =>
                !marble.Keywords.Contains("Category3")));

            GC.KeepAlive(xs);
            GC.KeepAlive(ys);
            GC.KeepAlive(zs);
        }

        #endregion ProxyFilterMessagesByKeywordsTest

        #region ProxyFilterChangeProxiesTest

        [TestMethod]
        public void ProxyFilterChangeProxiesTest()
        {
            const int SIZE = 1000;
            //arrange
            var proxyX = new CustomCountdownMonitorProxy("testX", SIZE + 1 /* + 1 = On Completed */);
            var proxyY = new CustomCountdownMonitorProxy("testY", SIZE + 1 /* + 1 = On Completed */);

            // Init
            Task<VisualRxInitResult> info =
                VisualRxSettings.Initialize(proxyX);
            info.Wait();

            // produce values
            var xs = Observable.Range(1, SIZE)
                .Monitor("X", 0)
                .Subscribe();

            // wait for completion
            //Assert.IsTrue(VisualRxSettings.WaitForProxiesCompletion(
            //    1  /* (streams) */,
            //    TimeSpan.FromSeconds(1)), "WaitForProxiesCompletion");

            // Init
            info = VisualRxSettings.Initialize(proxyY);
            info.Wait();

            // produce values
            var ys = Observable.Range(1, SIZE)
                .Monitor("Y", 1)
                .Subscribe();

            // wait for completion
            //Assert.IsTrue(VisualRxSettings.WaitForProxiesCompletion(
            //    1,
            //    TimeSpan.FromSeconds(1)), "WaitForProxiesCompletion");
            Assert.IsTrue(proxyX.Wait(), "Wait");
            Assert.IsTrue(proxyY.Wait(), "Wait");

            // validate
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxyX.Data.Count(), "proxyX.Data.Count = " + proxyX.Data.Count());
            Assert.AreEqual(SIZE + 1 /* + 1 = On Completed */, proxyY.Data.Count(), "proxyY.Data.Count = " + proxyY.Data.Count());

            GC.KeepAlive(xs);
            GC.KeepAlive(ys);
        }

        #endregion ProxyFilterChangeProxiesTest

        #region LateInitializaqtion

        [TestMethod]
        public void LateInitializaqtion()
        {
            const int SIZE = 1000;
            //arrange
            var proxy = new CustomCountdownMonitorProxy("test", 2 /* value + On Completed */);
            var xs = Observable.Return(1)
                .Monitor("Source", 0);

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize();
            info.Wait();

            // act
            xs.Wait();

            int countBefore = proxy.Data.Count();

            info = VisualRxSettings.AddProxies(proxy);
            info.Wait();

            xs.Wait();
            bool completed = proxy.Wait(500);
            Assert.IsTrue(completed, "Wait");
            int countAfter = proxy.Data.Count();

            // verify
            Assert.AreEqual(0, countBefore, "Count before proxy attachment");
            Assert.AreEqual(2, countAfter, "Count after proxy attachment"); // OnNext + OnComplete
        }

        #endregion LateInitializaqtion
    }
}