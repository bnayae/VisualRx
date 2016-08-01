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
    public class PartialPublicationTests
    {
        #region EnableDisableTest

        [TestMethod]
        public void EnableDisableTest()
        {
            var sync = new ManualResetEventSlim();
            VisualRxSettings.ClearFilters();

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create());

            VisualRxInitResult infos = info.Result;
            Trace.WriteLine(infos);

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(10);
            xs = xs.Monitor("Enable / Disable", 1);
            xs.Subscribe(
                v => VisualRxSettings.Enable = v < 3 || v > 6,
                () => sync.Set());

            sync.Wait();
            GC.KeepAlive(xs);
        }

        #endregion EnableDisableTest

        #region SplitToDifferentChannel_ByDatumType_Test

        [TestMethod]
        public void SplitToDifferentChannel_ByDatumType_Test()
        {
            VisualRxSettings.ClearFilters();

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create(),
                VisualRxWcfQueuedProxy.Create(),
                VisualRxTraceSourceProxy.Create());

            VisualRxSettings.AddFilter((marble, channelKey) =>
                {
                    bool enable = false;
                    switch (marble.Name)
                    {
                        case "Stream A":
                            enable = channelKey == VisualRxWcfDiscoveryProxy.KIND;
                            break;
                        case "Stream B":
                            enable = channelKey == VisualRxTraceSourceProxy.KIND;
                            break;
                        default:
                            enable = channelKey == VisualRxWcfQueuedProxy.KIND;
                            break;
                    }
                    return enable;
                });

            VisualRxInitResult infos = info.Result;
            Trace.WriteLine(infos);

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
               .Take(10);
            xs = xs.Monitor("Stream A", 1);
            var ys = Observable.Interval(TimeSpan.FromSeconds(1))
               .Take(10);
            ys = ys.Monitor("Stream B", 2);

            xs.Subscribe();
            //ys.Subscribe();
            ys.Wait();

            GC.KeepAlive(xs);
        }

        #endregion // SplitToDifferentChannel_ByDatumType_Test

        #region FilterByKeyword_Test

        [TestMethod]
        public void FilterByKeyword_Test()
        {
            VisualRxSettings.ClearFilters();

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create());

            VisualRxSettings.AddFilter((marble, channelKey) =>
                marble.Keywords.Contains("Key1"));

            VisualRxInitResult infos = info.Result;
            Trace.WriteLine(infos);

            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
               .Take(10);
            xs = xs.Monitor("Stream A", 1, "Key1", "Key3", "Key4");
            var ys = Observable.Interval(TimeSpan.FromSeconds(1))
               .Take(10);
            ys = ys.Monitor("Stream B", 2, "Key2", "Key3");

            //xs.Subscribe();
            ys.Subscribe();
            xs.Wait();

            GC.KeepAlive(xs);
        }

        #endregion // FilterByKeyword_Test

        public enum Category
        {
            BallGame,
            Snow,
            Extreme,
            Water,
            Weapon
        }

        private class Item
        {
            public Item(long value)
            {
                Value = value;
                this.Category = (Category)(value % 5);
            }
            public long Value { get; private set; }
            public Category Category { get; private set; }
        }
    }
}