#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class TaskTests : TestsBase
    {
        #region FromAsync_CancellationByTimeout_Test

        [TestMethod]
        public async Task ToObservable_CancellationByTimeout_Test()
        {
            var cts = new CancellationTokenSource();
            Task<string> t = PseudoDownloadAsync("http://blogs.microsoft.co.il/blogs/bnaya/archive/2012/07/05/intel-174-vtune-amplifier-xe.aspx", cts.Token);

            var xs = t.ToObservable()
                .Timeout(TimeSpan.FromMilliseconds(1));
            try
            {
                string data = await xs.FirstAsync();
                throw new NotSupportedException("the cancellation never happens");
            }
            catch (TimeoutException ex)
            {
                Trace.WriteLine(ex.Message);
            }

            //Assert.AreEqual(TaskStatus.Canceled, t.Status, "Task Status");

            // the task did not canceled
            Assert.AreEqual(TaskStatus.Running, t.Status, "Task Status");
        }

        #endregion FromAsync_CancellationByTimeout_Test

        #region FromAsync_CancellationByTimeout_Test

        [TestMethod]
        public void FromAsync_CancellationByTimeout_Test()
        {
            var xs = Observable.FromAsync(ct => PseudoDownloadAsync("http://blogs.microsoft.co.il/blogs/bnaya/archive/2012/07/05/intel-174-vtune-amplifier-xe.aspx", ct));
            xs = xs.Timeout(TimeSpan.FromSeconds(1));
            try
            {
                xs.First();
                Assert.Fail();
            }
            catch (TimeoutException ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        #endregion FromAsync_CancellationByTimeout_Test

        #region PseudoDownloadAsync

        public Task<string> PseudoDownloadAsync(string url, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {
                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < 2000)
                        token.ThrowIfCancellationRequested();
                    return "Html";
                });
        }

        #endregion PseudoDownloadAsync

        #region DownloadAsync

        public Task<string> DownloadAsync(string url, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();
            token.Register(() =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    client.CancelAsync();
                    Console.WriteLine("Canceled");
                }
            });

            client.DownloadStringCompleted += (s, e) =>
            {
                if (e.Cancelled)
                    tcs.SetCanceled();
                else if (e.Error != null)
                    tcs.SetException(e.Error);
                else
                    tcs.SetResult(e.Result);
            };

            client.DownloadStringAsync(new Uri(url));

            return tcs.Task;
        }

        #endregion DownloadAsync
    }
}