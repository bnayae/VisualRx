#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class ConnectableTests : TestsBase
    {
        #region IntervalTest

        [TestMethod]
        public void IntervalTest()
        {
            Observable.Timer(TimeSpan.FromMilliseconds(1)).Monitor("Start", 0).Subscribe();

            IObservable<long> xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(30);
            xs = xs.Monitor("Source", 1);

            IConnectableObservable<long> cs = xs.Publish();
            cs = cs.Monitor("Connectable", 2);
            cs.Subscribe();

            Thread.Sleep(2000);
            IDisposable connection = cs.Connect();

            Thread.Sleep(2000);
            connection.Dispose();

            Thread.Sleep(100);
            GC.KeepAlive(cs);
        }

        #endregion IntervalTest
    }
}