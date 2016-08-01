#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class SynchronizeTests : TestsBase
    {
        #region SynchronizePitfall

        [TestMethod]
        public void SynchronizePitfall()
        {
            var parallelStream = Observable.Create<int>(consumer =>
            {
                Parallel.For(0, 20, i => consumer.OnNext(i)); // publish on multi thread
                consumer.OnCompleted();
                return Disposable.Empty;
            });

            #region Monitor

            parallelStream = parallelStream.Monitor("Source", 1);

            #endregion Monitor

            var thdSafeStream = parallelStream.Synchronize(); // synchronize the On Next

            #region Monitor

            thdSafeStream = thdSafeStream.Monitor("Synchronize", 2);

            #endregion Monitor

            thdSafeStream.Wait();
        }

        #endregion SynchronizePitfall
    }
}