#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    public class CustomCountdownMonitorProxy : IVisualRxProxy
    {
        private ConcurrentQueue<MarbleBase> _queue = new ConcurrentQueue<MarbleBase>();
        private string _kind;
        private readonly CountdownEvent _sync;

        #region Ctor

        public CustomCountdownMonitorProxy(string kind, int numberOfExpectedElements)
        {
            _kind = kind;
            _sync = new CountdownEvent(numberOfExpectedElements);
        }

        #endregion Ctor

        #region Kind

        public string Kind
        {
            get { return _kind; }
        }

        #endregion Kind

        #region OnBulkSend

        /// <summary>
        /// Called when [bulk send].
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            foreach (var item in items)
            {
                _queue.Enqueue(item);
                _sync.Signal();
            }
        }

        #endregion OnBulkSend

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

        public void Dispose() { }

        #region Wait

        public bool Wait(int milliseconds = 4000)
        {
            return _sync.Wait(milliseconds);
        }

        #endregion Wait

        public IEnumerable<MarbleBase> Data { get { return _queue; } }
    }
}