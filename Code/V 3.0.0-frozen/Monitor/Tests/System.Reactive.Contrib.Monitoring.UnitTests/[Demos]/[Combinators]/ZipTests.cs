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
    public class ZipTests : TestsBase
    {
        #region WindowMinMaxTest

        [TestMethod]
        public void WindowMinMaxTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(12);
            xs = xs.Monitor("Interval", 1);
            var zs = from win in xs.Window(4).MonitorMany("Win", 2)
                            let min = win.Min().Monitor("Min", 3)
                                .Catch(Observable.Return(-1L))
                            let max = win.Max().Monitor("Max", 4).Catch(Observable.Return(-1L))
                            select Observable.Zip(min, max);
            
            var surrogate = new MonitorSurrogate<IList<long>>(
                    (items, marbles) => string.Format("Min = {0}, Max = {1}", items[0], items[1]));

            var fs = zs.Switch().Monitor("Zip", 5, surrogate);
            //zs.Subscribe();
            fs.Wait();
        }

        #endregion WindowMinMaxTest
    }
}