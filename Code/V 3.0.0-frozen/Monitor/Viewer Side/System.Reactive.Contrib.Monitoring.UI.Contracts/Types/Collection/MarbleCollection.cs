#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Data;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Custom observable collection with async capability
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MarbleCollection<T> : ObservableCollection<T>, IDisposable
    {
        #region Constants

        private const int DO_EVENT_MILLISECOND = 10;
        private const int NOTIFY_INTERVAL_MILLISECOND = 30;
        private const int BUFFER_SHOW_LIMIT = 100;

        private const string BUFFER_TEXT = "Buffering: ";
        private const string BUFFER_SIZE_PROPERTY = "BufferSize";
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private const DispatcherPriority DISPATCH_PRIORITY = DispatcherPriority.ApplicationIdle;
        private const DispatcherPriority DISPATCH_DO_EVENT_PRIORITY = DispatcherPriority.Inactive;

        #endregion // Constants

        #region Private / Protected Fields

        private Dispatcher _dispatcher = UIPluginHelper.CurrentDispatcher;
        private ConcurrentQueue<T> _addedItems = new ConcurrentQueue<T>();
        private readonly Timer _tmr;
        private readonly object _sync = new object();
        private int _subscriberCount = 0;
        private int _lastBuffer = 0;

        #endregion // Private / Protected Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleCollection&lt;T&gt;"/> class.
        /// </summary>
        public MarbleCollection()
        {
            _tmr = new Timer(Synchronize, null, NOTIFY_INTERVAL_MILLISECOND, NOTIFY_INTERVAL_MILLISECOND);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.MarbleCollection`1" /> class that contains elements copied from the specified collection.</summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> parameter cannot be null.</exception>
        public MarbleCollection(IEnumerable<T> collection)
            : base(collection)
        {
            _tmr = new Timer(Synchronize, null, NOTIFY_INTERVAL_MILLISECOND, NOTIFY_INTERVAL_MILLISECOND);
        }

        #endregion // Ctor

        #region CreateViewOnce

        /// <summary>
        /// Creates the view.
        /// </summary>
        private void CreateViewOnce()
        {
            #region Validation

            if (_view != null)
                throw new InvalidOperationException("You cannot create view more then once");

            #endregion // Validation

            Action createView = () => _view = (CollectionView)CollectionViewSource.GetDefaultView(this);
            _dispatcher.Invoke(createView, DispatcherPriority.Send);
        }

        #endregion // CreateViewOnce

        #region event NotifyCollectionChangedEventHandler CollectionChanged

        /// <summary>
        /// Occurs when [collection changed].
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                Interlocked.Increment(ref _subscriberCount);
                base.CollectionChanged += value;
            }
            remove
            {
                Interlocked.Decrement(ref _subscriberCount);
                base.CollectionChanged -= value;
            }
        }

        #endregion // event NotifyCollectionChangedEventHandler CollectionChanged

        #region Synchronize

        /// <summary>
        /// Synchronizes the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void Synchronize(object state)
        {
            if (_subscriberCount == 0)
                return;

            #region UI Sync

            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke((Action<object>)Synchronize, DISPATCH_PRIORITY, state);
                return;
            }

            #endregion // UI Sync

            var sw = Stopwatch.StartNew();
            T item;
            while (sw.ElapsedMilliseconds < DO_EVENT_MILLISECOND && // fairness
                _addedItems.TryDequeue(out item))
            {
                base.Add(item);
            }
            sw.Stop();
            RefreshBufferSize();

            if (_addedItems.Count > 0) // fairness
            {
                _dispatcher.BeginInvoke((Action<object>)Synchronize, DISPATCH_DO_EVENT_PRIORITY, state);
            }
        }

        #endregion // Synchronize

        #region BufferSize

        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        /// <value>
        /// The size of the buffer.
        /// </value>
        public string BufferSize
        {
            get
            {
                if (_addedItems.Count > BUFFER_SHOW_LIMIT)
                    return BUFFER_TEXT + _addedItems.Count;
                else
                    return string.Empty;
            }
        }

        #endregion // BufferSize

        #region RefreshBufferSize

        /// <summary>
        /// Refreshes the size of the buffer.
        /// </summary>
        private void RefreshBufferSize()
        {
            if (Math.Abs(_lastBuffer - _addedItems.Count) > 100)
            {
                Interlocked.Exchange(ref _lastBuffer, _addedItems.Count);
                OnPropertyChanged(BUFFER_SIZE_PROPERTY);
            }
        }

        #endregion // RefreshBufferSize

        #region OnPropertyChanged

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (_dispatcher.CheckAccess())
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            else
                _dispatcher.BeginInvoke((Action<string>)OnPropertyChanged, DISPATCH_PRIORITY, propertyName);
        }

        #endregion // OnPropertyChanged

        #region AddAsync

        /// <summary>
        /// Adds the async.
        /// </summary>
        /// <param name="item">item to add.</param>
        public void AddAsync(T item)
        {
            _addedItems.Enqueue(item);
            //_dispatcher.Invoke((Action<T>)base.Add, DISPATCH_PRIORITY, item);
        }

        /// <summary>
        /// Adds the async.
        /// </summary>
        /// <param name="item">item to add.</param>
        public void AddSync(T item)
        {
            //_addedItems.Enqueue(item);
            _dispatcher.Invoke((Action<T>)base.Add, DISPATCH_PRIORITY, item);
        }

        #endregion // AddAsync

        #region View

        private CollectionView _view;
        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public CollectionView View
        {
            get
            {
                if (_view == null)
                    CreateViewOnce();
                return _view;
            }
        }

        #endregion // View

        #region Dispose Pattern

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposed)
        {
            _tmr.Dispose();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="MarbleCollection&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~MarbleCollection()
        {
            Dispose(false);
        }

        #endregion // Dispose Pattern
    }
}