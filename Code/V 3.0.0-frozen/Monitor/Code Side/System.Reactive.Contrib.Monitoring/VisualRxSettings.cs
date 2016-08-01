#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

// TODO: consider parent relationships

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Visual Rx code side settings
    /// </summary>
    public static class VisualRxSettings
    {
        private static readonly object _syncProxies = new object();
        private static volatile bool _enable = true;

        /// <summary>
        /// collection of filters that gets marble and filter kind
        /// and return whether to forward
        /// the monitor information to this channel
        /// </summary>
        private static Func<MarbleCandidate, string, bool>[] _filters =
            new Func<MarbleCandidate, string, bool>[0];
        private static ConcurrentDictionary<string, Func<MarbleCandidate, string, bool>> _filterMap =
            new ConcurrentDictionary<string, Func<MarbleCandidate, string, bool>>();

        #region Initialize

        /// <summary>
        /// Initializes by code.
        /// </summary>
        /// <param name="proxies">proxies that will publish the monitor</param>
        /// <returns>Monitor proxies plug-in load information</returns>
        public static Task<VisualRxInitResult> Initialize(
            params IVisualRxProxy[] proxies)
        {
            #region Validation

            if (proxies == null)
            {
                var result = new VisualRxInitResult();
                return Task.FromResult<VisualRxInitResult>(result);
            }

            #endregion // Validation

            VisualRxProxyWrapper[] proxyWrappers = (from p in proxies
                                                    select new VisualRxProxyWrapper(p)).ToArray();

            lock (_syncProxies)
            {
                Proxies = proxyWrappers;
            }

            return GetProxiesInfoAsync(proxyWrappers);
        }

        #endregion Initialize

        #region GetProxiesInfoAsync

        /// <summary>
        /// Gets the proxies info a-sync.
        /// </summary>
        /// <param name="proxyWrappers">The proxy wrappers.</param>
        /// <returns></returns>
        private static Task<VisualRxInitResult> GetProxiesInfoAsync(VisualRxProxyWrapper[] proxyWrappers)
        {
            return Task.Factory.StartNew(() =>
            {
                var info = new VisualRxInitResult();
                foreach (VisualRxProxyWrapper proxy in proxyWrappers)
                {
                    var proxyInfo = info.Add(proxy);
                    try
                    {
                        string initInfo = proxy.Initialize();
                        proxyInfo.InitInfo = initInfo;
                    }
                    #region Exception Handling

                    catch (Exception ex)
                    {
                        proxyInfo.Error = ex;
                    }

                    #endregion Exception Handling
                }

                return info;
            });
        }

        #endregion // GetProxiesInfoAsync

        #region AddProxies

        /// <summary>
        /// Adds the proxies.
        /// </summary>
        /// <param name="proxies">The proxies.</param>
        /// <returns></returns>
        public static Task<VisualRxInitResult> AddProxies(params IVisualRxProxy[] proxies)
        {
            if (proxies == null || !proxies.Any())
                throw new ArgumentException("missing proxies");

            VisualRxProxyWrapper[] all;
            lock (_syncProxies)
            {
                IEnumerable<IVisualRxProxy> current = Proxies.Select(wrp => wrp.ActualProxy);
                IEnumerable<IVisualRxProxy> added = proxies.Except(current);

                IEnumerable<VisualRxProxyWrapper> addedWrappers =
                    from p in added
                    select new VisualRxProxyWrapper(p);

                all = (from w in Proxies.Union(addedWrappers) // // compare the memory address (same instance)
                       select w).ToArray();
                Proxies = all;
            }
            return GetProxiesInfoAsync(all);
        }

        #endregion // AddProxies

        #region RemoveProxies

        /// <summary>
        /// Removes the proxies.
        /// </summary>
        /// <param name="proxies">The proxies.</param>
        /// <exception cref="System.ArgumentException">missing proxies</exception>
        public static void RemoveProxies(params IVisualRxProxy[] proxies)
        {
            if (proxies == null || !proxies.Any())
                throw new ArgumentException("missing proxies");

            lock (_syncProxies)
            {
                IEnumerable<VisualRxProxyWrapper> left =
                    from w in Proxies
                    where !proxies.Any(p => p == w.ActualProxy)
                    select w;

                Proxies = left.ToArray();
            }
            Task.Factory.StartNew(state =>
                {
                    #region Dispose

                    var removed = state as IVisualRxProxy[];
                    foreach (var p in removed)
                    {
                        try
                        {
                            p.Dispose();
                        }
                        #region Exception Handling

                        catch (Exception ex)
                        {
                            TraceSourceMonitorHelper.Error("Dispose proxy: [{0}]\t{1}", p.Kind, ex);
                        }

                        #endregion // Exception Handling
                    }

                    #endregion // Dispose
                }, proxies);
        }

        #endregion // RemoveProxies

        #region AddFilter

        /// <summary>
        /// Adds the filter.
        /// </summary>
        /// <param name="filter">
        /// Gets marble and filter kind
        /// and return whether to forward
        /// the monitor information to this channel
        /// </param>
        /// <returns>the filter's key or null on failure</returns>
        public static string AddFilter(Func<MarbleCandidate, string, bool> filter)
        {
            string key = Guid.NewGuid().ToString();
            if (!_filterMap.TryAdd(key, filter))
            {
                TraceSourceMonitorHelper.Warn("Failed to add a filter");
                return null;
            }

            ExchangeFilters();
            return key;
        }

        #endregion AddFilter

        #region AddNamedFilter

        /// <summary>
        /// Adds the named filter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>success indication</returns>
        /// <remarks>may failed when trying to add a key multiple times</remarks>
        public static bool AddNamedFilter(string key, Func<MarbleCandidate, string, bool> filter)
        {
            if (!_filterMap.TryAdd(key, filter))
            {
                TraceSourceMonitorHelper.Warn("Failed to add a named filter ({0})", key);
                return false;
            }

            ExchangeFilters();
            return true;
        }

        #endregion // AddNamedFilter

        #region RemoveNamedFilter

        /// <summary>
        /// Adds the named filter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>success indication</returns>
        /// <remarks>may failed if not contain the key</remarks>
        public static bool RemoveNamedFilter(string key)
        {
            Func<MarbleCandidate, string, bool> filter;
            if (!_filterMap.TryRemove(key, out filter))
            {
                TraceSourceMonitorHelper.Warn("Failed to remove a named filter ({0})", key);
                return false;
            }

            ExchangeFilters();
            return true;
        }

        #endregion // RemoveNamedFilter

        #region ClearFilters

        /// <summary>
        /// Clears the filters.
        /// </summary>
        public static void ClearFilters()
        {
            _filterMap.Clear();
            ExchangeFilters();
        }

        #endregion ClearFilters

        #region ExchangeFilters

        /// <summary>
        /// Exchanges the filters (thread-safe no locking).
        /// </summary>
        private static void ExchangeFilters()
        {
            Func<MarbleCandidate, string, bool>[] oldFilters;
            Func<MarbleCandidate, string, bool>[] oldExchangedFilter;
            do
            {
                oldFilters = _filters;
                Func<MarbleCandidate, string, bool>[] newFilters = _filterMap.Values.ToArray();
                oldExchangedFilter =
                    Interlocked.CompareExchange(ref _filters, newFilters, oldFilters);
            } while (oldFilters != oldExchangedFilter);
        }

        #endregion // ExchangeFilters

        #region Filter

        /// <summary>
        /// Filters the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="proxyKind">ProxyKind</param>
        /// <returns></returns>
        internal static bool Filter(MarbleCandidate candidate, string proxyKind)
        {
            return !_filters.Any() ||
                _filters.All(filter => filter(candidate, proxyKind));
        }


        #endregion Filter

        #region GetProxies

        /// <summary>
        /// Gets the proxies.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        internal static VisualRxProxyWrapper[] GetProxies(MarbleCandidate candidate)
        {
            if (VisualRxSettings.Proxies == null || !VisualRxSettings.Proxies.Any())
            {
                TraceSourceMonitorHelper.Warn("MonitorOperator: No proxy found");
                return new VisualRxProxyWrapper[0];
            }

            var proxies =  (from p in VisualRxSettings.Proxies
                   where VisualRxSettings.Filter(candidate, p.Kind) &&
                        p.Filter(candidate)
                   select p).ToArray();
            return proxies;
        }

        #endregion // GetProxies

        #region Send

        /// <summary>
        /// Sends the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal static void Send(MarbleBase item, IEnumerable<VisualRxProxyWrapper> proxies)
        {
            #region Validation

            if (VisualRxSettings.Proxies == null || !VisualRxSettings.Proxies.Any())
            {
                TraceSourceMonitorHelper.Warn("MonitorOperator: No proxy found");
                return;
            }

            #endregion Validation

            foreach (VisualRxProxyWrapper proxy in proxies)
            {
                try
                {
                    //string kind = proxy.Kind;
                    //if (!VisualRxSettings.Filter(item, kind))
                    //    continue;

                    //if (!proxy.Filter(item))
                    //    continue;

                    // the proxy wrapper apply parallelism and batching (VIA Rx Subject)
                    proxy.Send(item);
                }
                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("MonitorOperator: {0}", ex);
                }

                #endregion Exception Handling
            }
        }

        #endregion Send

        #region Enable

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VisualRxSettings"/> is enable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enable; otherwise, <c>false</c>.
        /// </value>
        public static bool Enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
            }
        }

        #endregion Enable

        #region Proxies

        /// <summary>
        /// Gets the proxies.
        /// </summary>
        /// <value>
        /// The proxies.
        /// </value>
        private static VisualRxProxyWrapper[] Proxies { get; set; }

        #endregion Proxies

        #region CollectMachineInfo

        /// <summary>
        /// Gets or sets a value indicating whether to collect machine info like machine name and ip.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [collect machine info]; otherwise, <c>false</c>.
        /// </value>
        public static bool CollectMachineInfo { get; set; }

        #endregion CollectMachineInfo
    }
}