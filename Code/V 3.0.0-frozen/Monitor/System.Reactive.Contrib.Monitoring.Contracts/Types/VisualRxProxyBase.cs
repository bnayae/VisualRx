#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;

#endregion Using


namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// Visual Rx proxy's base class
    /// </summary>
    /// <remarks>
    /// doesn't use abstract member for IVisualRxFilterableProxy, because of performance consideration
    /// </remarks>
    public abstract class VisualRxProxyBase: IDisposable
    {
        #region Private / Protected Fields

        /// <summary>
        /// collection of filters that gets marble and filter kind
        /// and return whether to forward
        /// the monitor information to this channel
        /// </summary>
        private Func<MarbleCandidate, bool>[] _filters =
            new Func<MarbleCandidate, bool>[0];
        private ConcurrentDictionary<string, Func<MarbleCandidate, bool>> _filterMap =
            new ConcurrentDictionary<string, Func<MarbleCandidate, bool>>();

        #endregion // Private / Protected Fields

        #region AddFilter

        /// <summary>
        /// Adds a filter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>success indication</returns>
        /// <remarks>may failed when trying to add a key multiple times</remarks>
        public bool AddFilter(string key, Func<MarbleCandidate, bool> filter)
        {
            if (!_filterMap.TryAdd(key, filter))
            {
                TraceSourceMonitorHelper.Warn("Failed to add a named filter ({0})", key);
                return false;
            }

            ExchangeFilters();
            return true;
        }

        #endregion // AddFilter

        #region RemoveFilter

        /// <summary>
        /// remove a filter.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>success indication</returns>
        /// <remarks>may failed if not contain the key</remarks>
        public bool RemoveFilter(string key)
        {
            Func<MarbleCandidate, bool> filter;
            if (!_filterMap.TryRemove(key, out filter))
            {
                TraceSourceMonitorHelper.Warn("Failed to remove a named filter ({0})", key);
                return false;
            }

            ExchangeFilters();
            return true;
        }

        #endregion // RemoveFilter

        #region ClearFilters

        /// <summary>
        /// Clears the filters.
        /// </summary>
        public void ClearFilters()
        {
            _filterMap.Clear();
            ExchangeFilters();
        }

        #endregion ClearFilters

        #region ExchangeFilters

        /// <summary>
        /// Exchanges the filters (thread-safe no locking).
        /// </summary>
        private void ExchangeFilters()
        {
            Func<MarbleCandidate, bool>[] oldFilters;
            Func<MarbleCandidate, bool>[] oldExchangedFilter;
            do
            {
                oldFilters = _filters;
                Func<MarbleCandidate, bool>[] newFilters = _filterMap.Values.ToArray();
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
        /// <returns></returns>
        public bool Filter(MarbleCandidate candidate)
        {
            return !_filters.Any() ||
                _filters.All(filter => filter(candidate));
        }

        #endregion // Filter

        #region SendingThreshold

        /// <summary>
        /// Gets the sending threshold.
        /// </summary>
        /// <value>
        /// The sending threshold.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public SendThreshold SendingThreshold { get; set; }

        #endregion // SendingThreshold

        #region Finalizer and Dispose

        /// <summary>
        /// Performs application-defined tasks associated
        /// with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [SecuritySafeCritical]
        protected abstract void Dispose(bool disposed);

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="VisualRxProxyWrapper"/> is reclaimed by garbage collection.
        /// </summary>
        ~VisualRxProxyBase()
        {
            Dispose(false);
        }

        #endregion Finalizer and Dispose
    }
}