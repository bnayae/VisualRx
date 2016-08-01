#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Contrib.Monitoring.Contracts;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class BufferPartitioningTests : TestsBase
    {
        #region SimpleBufferTest

        [TestMethod]
        public void SimpleBufferTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(12);

            #region Monitor

            xs = xs.Monitor("Source", 1);

            #endregion Monitor

            var ws = xs.Buffer(4);

            #region Monitor

            var surrogate = new MonitorSurrogate<IList<long>> (
                (buffer, marble) => string.Join(",", buffer));
            ws = ws.Monitor("Buffered", 2, surrogate);

            #endregion Monitor

            ws.Wait();
        }

        #endregion SimpleBufferTest
    }
}