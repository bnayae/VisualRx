#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;

#endregion Using

// TODO: LIMIT the QueueSubject buffer size by config

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Visual Rx sender proxy
    /// Wrap the actual proxy and create a batch (bulk) send.
    /// </summary>
    internal sealed class VisualRxProxyWrapper : IDisposable
    {
        #region Constants

        private const int BUFFER_DURATION_MS = 400;
        private const int BUFFER_COUNT = 1000;

        #endregion Constants

        #region Private / Protected Fields

        private static readonly SendThreshold _defaultThreshold =
            new SendThreshold (TimeSpan.FromMilliseconds(BUFFER_DURATION_MS) , BUFFER_COUNT);

        private readonly IVisualRxProxy _actualProxy;
        private IVisualRxFilterableProxy _actualFilterableProxy;
        private ISubject<MarbleBase> _subject;
        private IDisposable _unsubSubject;

        private IScheduler _scheduler;

        #endregion Private / Protected Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualRxProxyWrapper" /> class.
        /// </summary>
        public VisualRxProxyWrapper(IVisualRxProxy actualProxy)
        {
            if (actualProxy == null)
                throw new ArgumentNullException("actualProxy");

            _actualFilterableProxy = actualProxy as IVisualRxFilterableProxy;

            _actualProxy = actualProxy;
        }

        #endregion // Ctor

        #region ActualProxy

        /// <summary>
        /// Gets the actual proxy.
        /// </summary>
        /// <value>
        /// The actual proxy.
        /// </value>
        internal IVisualRxProxy ActualProxy { get { return _actualProxy; } }

        #endregion // ActualProxy

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind { get { return _actualProxy.Kind; } }

        #endregion Kind

        #region Methods

        #region Initialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>Initialize information</returns>
        public string Initialize()
        {
            #region _scheduler = new EventLoopScheduler(...)

            _scheduler = new EventLoopScheduler(treadStart =>
                {
                    var trd = new Thread(treadStart);
                    trd.Priority = ThreadPriority.Normal;
                    trd.IsBackground = true;
                    trd.Name = "Monitor Proxy " + Kind;

                    return trd;
                });

            _scheduler.Catch<Exception>(e =>
                {
                    TraceSourceMonitorHelper.Error("Scheduling (OnBulkSend): {0}", e);
                    return true;
                });

            #endregion _scheduler = new EventLoopScheduler(...)

            SendThreshold threshhold = _actualProxy.SendingThreshold ?? _defaultThreshold;
            
            _subject = new Subject<MarbleBase>();
            var tmpStream = _subject
                .ObserveOn(_scheduler) // single thread
                //.Synchronize()
                .Retry()
                .Buffer(threshhold.WindowDuration, threshhold.WindowCount)
                .Where(items => items.Count != 0);
            _unsubSubject = tmpStream.Subscribe(_actualProxy.OnBulkSend);

            return _actualProxy.OnInitialize();
        }

        #endregion Initialize

        #region Send

        /// <summary>
        /// Sends the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Send(MarbleBase item)
        {
            _subject.OnNext(item);
        }

        #endregion Send

        #endregion Methods

        #region Finalizer and Dispose

        /// <summary>
        /// Performs application-defined tasks associated
        /// with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "DisposeInternal does call Dispose(disposed)")]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeInternal(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void DisposeInternal(bool disposed)
        {
            try
            {
                IDisposable unsubSubject = _unsubSubject;
                if (unsubSubject != null)
                    unsubSubject.Dispose();

                _actualProxy.Dispose();

                Dispose(disposed);
            }
            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("VisualRxTraceSourceProxy: {0}", ex);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposed) 
        {
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="VisualRxProxyWrapper"/> is reclaimed by garbage collection.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "DisposeInternal does call Dispose(disposed)")]
        ~VisualRxProxyWrapper()
        {
            DisposeInternal(false);
        }

        #endregion Finalizer and Dispose

        #region Filter

        /// <summary>
        /// Filters the specified item.
        /// </summary>
        /// <param name="candidate">The item.</param>
        /// <returns></returns>
        public bool Filter(MarbleCandidate candidate)
        {
            if (_actualFilterableProxy == null)
                return true;
            return _actualFilterableProxy.Filter(candidate);
        }

        #endregion // Filter
    }
}