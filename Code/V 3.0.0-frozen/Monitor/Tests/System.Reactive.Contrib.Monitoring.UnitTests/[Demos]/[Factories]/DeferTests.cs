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

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class DeferTests : TestsBase
    {
        #region DeferVsDeferAsyncTest

        [TestMethod]
        public void DeferVsDeferAsyncTest()
        {
            const string NOT_AUTH = "not authenticate";

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(20)
                .Publish(); // convert into hot observable
            xs.Monitor("Source", 1).Subscribe();
            xs.Connect();

            var ds = Observable.Defer(() => xs);
            ds = ds.Monitor("Defer", 2);

            var ds1 = Observable.DeferAsync<long>((ct) =>
                {
                    return Task.Factory.StartNew(() =>
                        {
                            if (Authenticate("user", ct))
                                return xs;
                            else
                                return Observable.Throw<long>(new SecurityException("not authenticate"));
                        });
                });
            ds1 = ds1.Monitor("Defer Async User", 3);

            var ds2 = Observable.DeferAsync<long>((ct) =>
                {
                    return Task.Factory.StartNew(() =>
                        {
                            if (Authenticate("admin", ct))
                                return xs;
                            else
                                return Observable.Throw<long>(new SecurityException(NOT_AUTH));
                        });
                });
            ds2 = ds2.Monitor("Defer Async Admin", 4);

            ds.Subscribe();
            ds1.Subscribe();
            ds2.Subscribe();

            try
            {
                ds.Wait();
                ds1.Wait();
                ds2.Wait();
            }

            #region Exception Handling

            catch (SecurityException ex)
            {
                Assert.AreEqual(NOT_AUTH, ex.Message);
            }

            #endregion Exception Handling
        }

        #endregion DeferVsDeferAsyncTest

        #region Authenticate

        private bool Authenticate(string name, CancellationToken ct)
        {
            Thread.Sleep(2000);
            if (ct.IsCancellationRequested)
                return false;
            return name == "admin";
        }

        #endregion Authenticate
    }
}