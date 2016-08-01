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
    public class TestScopeFactory : IVisualRxProxy
    {
        private const string KIND = "Custom";

        private event EventHandler Completion = (s, e) => { };

        #region Create

        /// <summary>
        /// Creates a scope that will be release after n times of completion or errors.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <returns></returns>
        public IDisposable Create(int initialCount = 1)
        {
            return new MonitorScope(this, initialCount);
        }

        #endregion Create

        #region Kind

        public string Kind
        {
            get { return KIND; }
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
                if (item.Kind == MarbleKind.OnCompleted ||
                    item.Kind == MarbleKind.OnError)
                {
                    Completion(this, EventArgs.Empty);
                }
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

        /// <summary>
        /// Dispose after n completion or on errors
        /// </summary>
        private class MonitorScope : IDisposable
        {
            private readonly CountdownEvent _sync;
            private TestScopeFactory _proxy;

            #region Ctor

            public MonitorScope(TestScopeFactory proxy, int initialCount)
            {
                _sync = new CountdownEvent(initialCount);
                _proxy = proxy;
                proxy.Completion += OnCompletionHandler;
            }

            #endregion Ctor

            #region OnCompletionHandler

            private void OnCompletionHandler(object sender, EventArgs e)
            {
                _sync.Signal();
            }

            #endregion OnCompletionHandler

            #region Dispose

            void IDisposable.Dispose()
            {
                _sync.Wait();
                _proxy.Completion -= OnCompletionHandler;
                _proxy = null;
            }

            #endregion Dispose
        }

    }
}