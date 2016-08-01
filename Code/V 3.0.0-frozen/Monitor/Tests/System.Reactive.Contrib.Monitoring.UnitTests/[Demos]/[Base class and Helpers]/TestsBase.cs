#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class TestsBase
    {
        protected TestScopeFactory ScopeFactory { get; private set; }

        #region TestInitialize

        [TestInitialize()]
        public virtual void TestInitialize()
        {
            VisualRxSettings.ClearFilters();

            ScopeFactory = new TestScopeFactory();
            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create(),
                //VisualRxWcfQueuedProxy.Create(),
                //VisualRxTraceSourceProxy.Create(),
                ScopeFactory);

            VisualRxInitResult infos = info.Result;
            Trace.WriteLine(infos);
        }

        #endregion TestInitialize
    }
}