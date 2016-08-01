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
    public class LookupAndDictionaryTests : TestsBase
    {
        #region LookupTest

        [TestMethod]
        public void LookupTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.1))
                .Take(100);

            xs = xs.Monitor("A", 1);

            var zs = from w in xs.Window(10)
                     from lookups in w.ToLookup<long, string>(i => (i % 4).ToString())
                     select lookups;

            #region zs = zs.Monitor("Lookup",...)
                        
            var surrogate = new MonitorSurrogate<ILookup<string, long>>(
                (lookup, marble) =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IGrouping<string, long> g in lookup)
                    {
                        sb.AppendFormat("{0}: ", g.Key);
                        foreach (var item in g)
                        {
                            sb.AppendFormat("{0},", item);
                        }
                        sb.AppendLine();
                    }
                    return sb.ToString();
                });
            zs = zs.Monitor("Lookup", 1, surrogate);

            #endregion zs = zs.Monitor("Lookup",...)

            zs.Wait();
        }

        #endregion LookupTest

        #region ToDictionaryTest

        [TestMethod]
        public void ToDictionaryTest()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.1))
                .Take(100);

            xs = xs.Monitor("A", 1);

            var zs = from w in xs.Window(10)
                     from lookups in w.ToDictionary<long, string>(i => (i).ToString())
                     select lookups;

            #region zs = zs.Monitor("ToDictionary",...)

            var surrogate = new MonitorSurrogate<IDictionary<string, long>>(
                (lookup, marble) =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, long> pair in lookup)
                    {
                        sb.AppendFormat("{0}: {1}\r\n", pair.Key, pair.Value);
                    }
                    return sb.ToString();
                });

            zs = zs.Monitor("ToDictionary", 1, surrogate);

            #endregion zs = zs.Monitor("ToDictionary",...)

            zs.Wait();
        }

        #endregion ToDictionaryTest

        #region ToDictionary_Fault_Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ToDictionary_Fault_Test()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.1))
                .Take(100);

            xs = xs.Monitor("A", 1);

            var zs = from w in xs.Window(10)
                     from lookups in w.ToDictionary<long, string>(i => (i % 7).ToString())
                     select lookups;

            #region zs = zs.Monitor("ToDictionary",...)

            var surrogate = new MonitorSurrogate<IDictionary<string, long>>(
                (lookup, marble) =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, long> pair in lookup)
                    {
                        sb.AppendFormat("{0}: {1}\r\n", pair.Key, pair.Value);
                    }
                    return sb.ToString();
                });
            zs = zs.Monitor("ToDictionary", 1, surrogate);

            #endregion zs = zs.Monitor("ToDictionary",...)
           
            await zs;
        }

        #endregion ToDictionary_Fault_Test
    }
}