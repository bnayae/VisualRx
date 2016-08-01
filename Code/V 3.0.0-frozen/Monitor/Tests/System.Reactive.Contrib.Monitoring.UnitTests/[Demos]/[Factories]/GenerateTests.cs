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
    public class GenerateTests : TestsBase
    {
        #region TimeExtend

        [TestMethod]
        public void TimeExtend()
        {

            IObservable<string> xs = Observable.Generate(
                1, // init,
                i => i < 10, // condition
                i => i + 1, // iterate
                i => new string('*', i), // select
                i => TimeSpan.FromMilliseconds(i * 200));

           xs.Monitor("Generate", 1).Wait();
        }

        #endregion TimeExtend
    }
}