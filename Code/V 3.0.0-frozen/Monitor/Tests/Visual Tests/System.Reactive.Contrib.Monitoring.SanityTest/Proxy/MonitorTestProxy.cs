#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

// TODO: LIMIT the QueueSubject buffer size

namespace System.Reactive.Contrib.Profiling.Proxies
{
    /// <summary>
    /// monitorr proxy for tests
    /// </summary>
    public class MonitorTestProxy : IVisualRxProxy
    {
        public const string KIND = "Test";

        private ConcurrentQueue<MarbleBase> _queue = new ConcurrentQueue<MarbleBase>();

        #region OnBulkSend

        /// <summary>
        /// Bulk send.
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            foreach (var item in items)
            {
                _queue.Enqueue(item);
            }
        }

        #endregion OnBulkSend

        #region Data

        /// <summary>
        /// Gets the data.
        /// </summary>
        public IEnumerable<MarbleBase> Data { get { return _queue; } }

        #endregion Data

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind { get { return KIND; } }

        #endregion Kind

        #region OnInitialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>
        /// initialize information
        /// </returns>
        public string OnInitialize()
        {
            return string.Empty; 
        }

        #endregion // OnInitialize

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

        #region Create

        /// <summary>
        /// Creates proxy.
        /// </summary>
        /// <returns></returns>
        public static MonitorTestProxy Create()
        {
            return new MonitorTestProxy();
        }

        #endregion Create

        public void Dispose() { }
    }
}