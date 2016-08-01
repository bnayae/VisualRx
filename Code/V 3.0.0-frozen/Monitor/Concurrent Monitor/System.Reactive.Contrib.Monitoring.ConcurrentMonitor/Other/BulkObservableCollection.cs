using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Bulk observable collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        #region Private / Protected Fields

        private const int SCHEDULE_UI_THRESHOLD = 200;
        private const int SCHEDULE_UI_REMOVE_THRESHOLD = 350;
        private readonly object SCHEDULED_UI = new object();

        private object _isScheduledUI = null;
        private delegate void AddRangeCallback(IList<T> items);
        private delegate void SetItemCallback(int index, T item);
        private delegate void RemoveItemCallback(int index);
        private delegate void ClearItemsCallback();
        private delegate void InsertItemCallback(int index, T item);
        private delegate void MoveItemCallback(int oldIndex, int newIndex);
        private int _rangeOperationCount;
        //private bool _collectionChangedDuringRangeOperation;
        private Dispatcher _dispatcher;
        private ReadOnlyObservableCollection<T> _readOnlyAccessor;
        private static readonly NotifyCollectionChangedEventArgs REASET_ARGS =
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        private readonly ConcurrentQueue<T> _buffer = new ConcurrentQueue<T>();

        private bool _cleaning;

        #endregion // Private / Protected Fields

        #region Ctor

        public BulkObservableCollection()
        {
            this._dispatcher = Dispatcher.CurrentDispatcher;
        }

        public BulkObservableCollection(IEnumerable<T> items)
            : this()
        {
            AddRange(items);
        }

        #endregion // Ctor

        #region RunOnUI

        public void RunOnUI(Action action)
        {
            if (this._dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                this._dispatcher.BeginInvoke(DispatcherPriority.Background,
                            (Action)action);
            }
        }

        #endregion // RunOnUI

        #region Paused

        private bool _paused;

        public bool Paused
        {
            get { return _paused; }
            set
            {
                _paused = value;
                if (value)
                    StartScheduleUI();

                OnPropertyChanged();
            }
        }

        #endregion // Paused

        #region INotifyPropertyChanged Members and overrides

        protected override event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(args);
        }

        #endregion // INotifyPropertyChanged Members and overrides

        #region AddRange

        public void AddRange(params T[] items)
        {
            AddRange(items as IEnumerable<T>);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;
            if (this._dispatcher.CheckAccess())
            {
                Task t = Task.Run(() => AddRange(items));
                return;
            }

            foreach (var item in items)
            {
                _buffer.Enqueue(item);
            }

            Thread.MemoryBarrier();
            if (!_paused && !_cleaning)
                StartScheduleUI();
        }

        #endregion // AddRange

        #region RemoveWhile

        public async void RemoveWhile(Predicate<T> stalenessFilter)
        {
            if (_cleaning)
                return;

            if (this._dispatcher.CheckAccess())
            {
                try
                {
                    try
                    {
                        _cleaning = true;
                        Thread.MemoryBarrier();

                        this.BeginBulkOperation();
                        while (base.Count > 0 && stalenessFilter(base[0]))
                        {
                            base.RemoveAt(0);
                        }
                    }
                    finally
                    {
                        EndBulkOperation();
                    }

                    await Task.Run(() =>
                        {
                            T item;
                            while (_buffer.TryPeek(out item) && stalenessFilter(item))
                                _buffer.TryDequeue(out  item);
                        });
                }
                finally
                {
                    _cleaning = false;
                }

                if (!_paused)
                    StartScheduleUI();
            }
            else
                await _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action<Predicate<T>>)(RemoveWhile), stalenessFilter);
        }

        public void RemoveItems(T[] items)
        {
            RemoveItemsInternal(items, 0);
        }

        private void RemoveItemsInternal(T[] items, int index)
        {
            if (this._dispatcher.CheckAccess())
            {
                try
                {
                    this.BeginBulkOperation();
                    var sw = Stopwatch.StartNew();
                    int len = items.Length;
                    while (index < len && sw.ElapsedMilliseconds < SCHEDULE_UI_REMOVE_THRESHOLD)
                    {
                        T item = items[index];
                        base.Remove(item);
                        index++;
                    }
                }
                finally
                {
                    EndBulkOperation();
                }

                if (index >= items.Length)
                    return;
            }

            _dispatcher.BeginInvoke(DispatcherPriority.Background, (Action<T[], int>)(RemoveItemsInternal), items, index);
        }

        #endregion // RemoveWhile

        #region StartScheduleUI

        private void StartScheduleUI()
        {
            object local = Interlocked.CompareExchange(ref _isScheduledUI, SCHEDULED_UI, null);
            if (local == null)
                _dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)ScheduleUI);
        }

        #endregion // StartScheduleUI

        #region ScheduleUI

        private void ScheduleUI()
        {
            Interlocked.Exchange(ref _isScheduledUI, null); // let rescheduling occurs
            var sw = Stopwatch.StartNew();
            try
            {
                this.BeginBulkOperation();

                bool exitBeforeTime = false;
                T item;
                while (!_paused && !_cleaning && _buffer.TryDequeue(out item))
                {
                    base.Add(item);

                    #region Validation

                    if (sw.ElapsedMilliseconds > SCHEDULE_UI_THRESHOLD)
                    {
                        exitBeforeTime = true;
                        break;
                    }

                    #endregion // Validation
                }

                if (exitBeforeTime)
                    StartScheduleUI();
            }
            finally
            {
                sw.Stop();
                EndBulkOperation();
                Thread.MemoryBarrier();
            }
        }

        #endregion // ScheduleUI

        #region BeginBulkOperation

        public void BeginBulkOperation()
        {
            this._rangeOperationCount++;
        }

        #endregion // BeginBulkOperation

        #region EndBulkOperation

        public void EndBulkOperation()
        {
            if (_rangeOperationCount > 0 && --_rangeOperationCount == 0) // && _collectionChangedDuringRangeOperation)
            {
                OnCollectionChanged(REASET_ARGS);
                //_collectionChangedDuringRangeOperation = false;
            }
        }

        #endregion // EndBulkOperation

        #region AsReadOnly

        public ReadOnlyObservableCollection<T> AsReadOnly()
        {
            if (this._readOnlyAccessor == null)
            {
                this._readOnlyAccessor = new ReadOnlyObservableCollection<T>(this);
            }
            return this._readOnlyAccessor;
        }

        #endregion // AsReadOnly

        #region OnCollectionChanged

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_rangeOperationCount == 0)
            {
                base.OnCollectionChanged(e);
                return;
            }
            //_collectionChangedDuringRangeOperation = true;
        }

        #endregion // OnCollectionChanged

        #region SetItem

        protected override void SetItem(int index, T item)
        {
            if (this._dispatcher.CheckAccess())
            {
                base.SetItem(index, item);
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, new BulkObservableCollection<T>.SetItemCallback(this.SetItem), index, new object[]
                  {
                        item
                  });
        }

        #endregion // SetItem

        #region InsertItem

        protected override void InsertItem(int index, T item)
        {
            if (this._dispatcher.CheckAccess())
            {
                base.InsertItem(index, item);
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, new BulkObservableCollection<T>.InsertItemCallback(this.InsertItem), index, new object[]
                  {
                        item
                  });
        }

        #endregion // InsertItem

        #region MoveItem

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (this._dispatcher.CheckAccess())
            {
                base.MoveItem(oldIndex, newIndex);
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, new BulkObservableCollection<T>.MoveItemCallback(this.MoveItem), oldIndex, new object[]
                  {
                        newIndex
                  });
        }

        #endregion // MoveItem

        #region RemoveItem

        protected override void RemoveItem(int index)
        {
            if (this._dispatcher.CheckAccess())
            {
                base.RemoveItem(index);
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, new BulkObservableCollection<T>.RemoveItemCallback(this.RemoveItem), index);
        }

        #endregion // RemoveItem

        #region RemoveSafe

        public void RemoveSafe(T item)
        {
            if (this._dispatcher.CheckAccess())
            {
                base.Remove(item);
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, (Action<T>)(this.RemoveSafe), item);
        }

        #endregion // RemoveSafe

        #region ClearItems

        protected override void ClearItems()
        {
            if (this._dispatcher.CheckAccess())
            {
                base.ClearItems();
                return;
            }
            this._dispatcher.BeginInvoke(DispatcherPriority.Send, new BulkObservableCollection<T>.ClearItemsCallback(this.ClearItems));
        }

        #endregion // ClearItems
    }
}
