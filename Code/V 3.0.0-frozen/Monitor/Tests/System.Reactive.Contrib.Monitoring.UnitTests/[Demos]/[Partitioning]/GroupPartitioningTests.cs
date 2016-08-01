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

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class ConcurrentGroupPartitioningTests : TestsBase
    {
        #region GroupTest

        [TestMethod]
        public void GroupTest()
        {
            var scheduler = new EventLoopScheduler();
            var xs = Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
                .Take(20);

            #region Monitor

            xs = xs.Monitor("Source", 1);

            #endregion Monitor

            var ws = from item in xs
                     group item by (item % 5) into g
                     select g;

            #region Monitor

            ws = ws.MonitorGroup("Grp", 2);

            #endregion Monitor

            ws.Wait();
            Thread.Sleep(100);
        }

        #endregion GroupTest


        #region GroupAndKeywordsTest

        [TestMethod]
        public void GroupAndKeywordsTest()
        {
            var inputA1 = Observable.Interval(TimeSpan.FromSeconds(1.5)).Take(7);
            inputA1 = inputA1.Monitor("Input A1", 1, "ScenarioA", "Input");
            var inputA2 = Observable.Interval(TimeSpan.FromSeconds(2)).Take(5);
            inputA2 = inputA2.Monitor("Input A2", 2, "ScenarioA", "Input");
            var outputA = Observable.Merge(inputA1, inputA2);
            outputA = outputA.Monitor("Output A", 3, "ScenarioA", "Output");

            var inputB = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
            inputB = inputB.Monitor("Input B", 4, "ScenarioB", "Input");
            var outputB = from item in inputB
                          group item by item % 3 into g
                          select g;
            outputB = outputB.MonitorGroup("Output B", 5, "ScenarioB", "Output");

            //outputA.Subscribe();
            outputB.Subscribe(g => g.Subscribe());

            outputA.Wait();
            GC.KeepAlive(outputB);
            Thread.Sleep(100);
        }

        #endregion GroupAndKeywordsTest

    }
}