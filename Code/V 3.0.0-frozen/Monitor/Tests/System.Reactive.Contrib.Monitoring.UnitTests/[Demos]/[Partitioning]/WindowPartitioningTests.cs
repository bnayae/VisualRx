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
    public class WindowPartitioningTests : TestsBase
    {
        #region SimpleWindowTest

        [TestMethod]
        public void SimpleWindowTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(12);

            #region Monitor

            xs = xs.Monitor("Source", 1);

            #endregion Monitor

            var ws = xs.Window(3);

            #region Monitor

            ws = ws.MonitorMany("Win", 2);

            #endregion Monitor

            ws.Switch().Wait();
        }

        #endregion SimpleWindowTest

        #region WindowAggregationTest

        [TestMethod]
        public void WindowAggregationTest()
        {
            IObservable<long> xs = Observable.Interval(TimeSpan.FromSeconds(0.01))
                .Take(300);

            #region Monitor

            xs = xs.Monitor("Source", 1);

            #endregion Monitor

            IObservable<IObservable<long>> ws = xs.Window(TimeSpan.FromSeconds(2));

            #region Monitor

            ws = ws.MonitorMany("Win", 2);

            #endregion Monitor

            IObservable<OHLC> aggs = from win in ws
                                     from acc in win.Aggregate(new OHLC(), OHLC.Accumulate)
                                     select acc;

            #region Monitor

            var surrogate = new MonitorSurrogate<OHLC>(
                (holc, marble) => string.Format("Open = {0}, Close = {1}, High = {2}, Low = {3}",
                                                holc.Open, holc.Close, holc.High, holc.Low));

            aggs = aggs.Monitor("Accumulate", 3, surrogate);

            #endregion Monitor

            aggs.Wait();
        }

        #region OHLC

        // credit: http://enumeratethis.com/2011/07/26/financial-charts-reactive-extensions/
        private class OHLC
        {
            public double? Open { get; private set; }
            public double? High { get; private set; }
            public double? Low { get; private set; }
            public double Close { get; private set; }

            public static OHLC Accumulate(OHLC state, long price)
            {
                // Take the current values & apply the price update.    
                state.Open = state.Open ?? price;
                state.High = state.High.HasValue ? state.High > price ? state.High : price : price;
                state.Low = state.Low.HasValue ? state.Low < price ? state.Low : price : price;
                state.Close = price;
                return state;
            }
        }

        #endregion // OHLC

        #endregion WindowAggregationTest

        #region SlidingWindowTest

        [TestMethod]
        public void SlidingWindowTest()
        {
            var sync = new ManualResetEventSlim();
            var scheduler = new EventLoopScheduler();
            var xs = Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
                .Take(20);

            #region Monitor

            xs = xs.Monitor("Source", 1);

            #endregion Monitor

            var ws = xs.Window(3, 1);

            #region Monitor

            ws = ws.MonitorMany("Win", 2);

            #endregion Monitor

            ws.Subscribe(w =>
                {
                    w.Subscribe();
                },
                () => sync.Set());
            //ws.Switch().Wait();
            sync.Wait();
        }

        #endregion SlidingWindowTest
    }
}