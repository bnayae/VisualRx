#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Disposables;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class CreateTests : TestsBase
    {
        #region WellBehavedEnforcementTest

        [TestMethod]
        public void WellBehavedEnforcementTest()
        {

           IObservable<int> xs = Observable.Create<int>(observer =>
            {
                observer.OnNext(1);
                observer.OnCompleted();
                observer.OnNext(2);
                var disp = new CancellationDisposable();
                disp.Token.Register(() => Trace.WriteLine("Canelled"));
                return disp;
            });

           xs.Monitor("Create", 1).Wait();
        }

        #endregion WellBehavedEnforcementTest
    }
}