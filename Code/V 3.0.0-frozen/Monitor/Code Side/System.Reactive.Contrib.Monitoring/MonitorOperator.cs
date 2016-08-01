#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

// TODO: consider parent relationships

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Main class for the monitor logic and coordination
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MonitorOperator<T>
    {
        #region Constants

        private const string QUEUE_SUBJECT_NAME = "Rx Monitor";

        #endregion Constants

        // TODO: safe stopwatch
        //private static ThreadLocal<Stopwatch> _stopwatch = new ThreadLocal<Stopwatch>(() => Stopwatch.StartNew());

        #region Private / Protected Fields

        private readonly string _name;
        private readonly string _machineName;
        private readonly string[] _keyworkds;
        private readonly double _indexOrder;
        private readonly IMonitorSurrogate<T> _surrogate;
        private static readonly DefaultSurrogate<T> _defaultSurrogate = new DefaultSurrogate<T>();

        #endregion Private / Protected Fields

        #region Constructors

        public MonitorOperator(
            string name,
            double indexOrder,
            IMonitorSurrogate<T> surrogate,
            string[] keywords)
        {
            #region Validation

            if (keywords == null)
                keywords = new string[0];

            if (keywords == null)
                keywords = new string[0];

            if (surrogate == null)
                surrogate = _defaultSurrogate;

            #endregion Validation

            _name = name;
            _surrogate = surrogate;
            _keyworkds = keywords;
            _indexOrder = indexOrder;
            try
            {
                if (VisualRxSettings.CollectMachineInfo)
                {
                    _machineName = Environment.MachineName;
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Warn("fail to collect machine information {0}", ex);
            }

            #endregion Exception Handling
        }

        #endregion Constructors

        #region Methods

        #region GetLocalIPs

        /// <summary>
        /// Gets the local I ps.
        /// </summary>
        /// <returns></returns>
        private string GetLocalIPs()
        {
            string sHostName = Dns.GetHostName();
            IPAddress[] IpA = Dns.GetHostAddresses(sHostName);
            string[] results = IpA.Select(address => address.ToString()).ToArray();
            return string.Join(";", results);
        }

        #endregion // GetLocalIPs

        #region AttachTo

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public IObservable<T> AttachTo(IObservable<T> instance)
        {
            return instance.Do(PublishOnNext, PublishError, PublishComplete);
        }

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public IConnectableObservable<T> AttachTo(IConnectableObservable<T> instance)
        {
            IObservable<T> monitorStream = instance.Do(PublishOnNext, PublishError, PublishComplete);
            return new ConnectableWraper(monitorStream, instance);
        }

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public ISubject<T> AttachTo(ISubject<T> instance)
        {
            IObservable<T> monitorStream = instance.Do(PublishOnNext, PublishError, PublishComplete);
            return new SubjectWrapper(monitorStream, instance);
        }

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public ISubject<TIn, T> AttachTo<TIn>(ISubject<TIn, T> instance)
        {
            var monitorStream = instance.Do(PublishOnNext, PublishError, PublishComplete);
            return new SubjectWrapper<TIn, T>(monitorStream, instance);
        }

        #endregion AttachTo

        #region Publish

        #region PublishOnNext

        /// <summary>
        /// Publishes the on next.
        /// </summary>
        /// <param name="value">The value.</param>
        public void PublishOnNext(T value)
        {
            if (!VisualRxSettings.Enable)
                return;

            var candidate = new MarbleCandidate(_name, MarbleKind.OnNext, _keyworkds);

            VisualRxProxyWrapper[] proxies = VisualRxSettings.GetProxies(candidate);

            #region Validation

            if (proxies.Length == 0)
                return;

            #endregion // Validation

            object item = _surrogate.Mapping(value, candidate) ?? value;
            // TODO: Marble base should have a virtual method for the surrogate
            var marbleItem = MarbleNext.Create(_name, _surrogate.SerializationStrategy, item,
                SafeStopwatch.Elapsed(), _machineName);
            Publish(marbleItem, proxies);
        }

        #endregion PublishOnNext

        #region PublishComplete

        /// <summary>
        /// Publishes the complete.
        /// </summary>
        public void PublishComplete()
        {
            if (!VisualRxSettings.Enable)
                return;

            var candidate = new MarbleCandidate(_name, MarbleKind.OnCompleted, _keyworkds);
            VisualRxProxyWrapper[] proxies = VisualRxSettings.GetProxies(candidate);

            #region Validation

            if (proxies.Length == 0)
                return;

            #endregion // Validation

            var marbleItem = MarbleComplete.Create(_name, _surrogate.SerializationStrategy, SafeStopwatch.Elapsed(),
                _machineName);
            Publish(marbleItem, proxies);
        }

        #endregion PublishComplete

        #region PublishError

        /// <summary>
        /// Publishes the error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void PublishError(Exception ex)
        {
            if (!VisualRxSettings.Enable)
                return;

            var candidate = new MarbleCandidate(_name, MarbleKind.OnError, _keyworkds);
            VisualRxProxyWrapper[] proxies = VisualRxSettings.GetProxies(candidate);

            #region Validation

            if (proxies.Length == 0)
                return;

            #endregion // Validation

            var marbleItem = MarbleError.Create(_name, _surrogate.SerializationStrategy, ex, SafeStopwatch.Elapsed()
                , _machineName);
            Publish(marbleItem, proxies);
        }

        #endregion PublishError

        /// <summary>
        /// Publishes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void Publish(MarbleBase item, VisualRxProxyWrapper[] proxies)
        {
            item.Keywords = _keyworkds;
            item.IndexOrder = _indexOrder;

            VisualRxSettings.Send(item, proxies);
        }

        #endregion Publish

        #endregion Methods

        #region Nested Types

        #region ConnectableWraper

        /// <summary>
        /// Connectable observable monitor wrapper
        /// </summary>
        private class ConnectableWraper : IConnectableObservable<T>
        {
            private IConnectableObservable<T> _internalStream;
            private IObservable<T> _monitorStream;

            #region Ctor

            public ConnectableWraper(IObservable<T> monitorStream, IConnectableObservable<T> internalStream)
            {
                _internalStream = internalStream;
                _monitorStream = monitorStream;
            }

            #endregion Ctor

            #region Connect

            /// <summary>
            /// start the observable.
            /// </summary>
            /// <returns></returns>
            public IDisposable Connect()
            {
                return _internalStream.Connect();
            }

            #endregion Connect

            #region Subscribe

            /// <summary>
            /// Subscribes the specified observer.
            /// </summary>
            /// <param name="observer">The observer.</param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<T> observer)
            {
                var callbackDisposable = new CancellationDisposable();
                IDisposable disp = _monitorStream
                    .Finally(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        })
                    .Subscribe(observer);
                callbackDisposable.Token.Register(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        });
                return new CompositeDisposable(callbackDisposable, disp);
            }

            #endregion Subscribe
        }

        #endregion ConnectableWraper

        #region SubjectWrapper

        /// <summary>
        /// Connectable observable monitor wrapper
        /// </summary>
        private class SubjectWrapper : ISubject<T>
        {
            private ISubject<T> _internalStream;
            private IObservable<T> _monitorStream;

            #region Ctor

            public SubjectWrapper(IObservable<T> monitorStream, ISubject<T> internalStream)
            {
                _internalStream = internalStream;
                _monitorStream = monitorStream;
            }

            #endregion Ctor

            #region Subscribe

            /// <summary>
            /// Subscribes the specified observer.
            /// </summary>
            /// <param name="observer">The observer.</param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<T> observer)
            {
                var callbackDisposable = new CancellationDisposable();
                IDisposable disp = _monitorStream
                    .Finally(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        })
                    .Subscribe(observer);
                callbackDisposable.Token.Register(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        });
                return new CompositeDisposable(callbackDisposable, disp);
            }

            #endregion Subscribe

            #region IObserver Members

            /// <summary>
            /// Called when [completed].
            /// </summary>
            public void OnCompleted()
            {
                _internalStream.OnCompleted();
            }

            /// <summary>
            /// Called when [error].
            /// </summary>
            /// <param name="error">The error.</param>
            public void OnError(Exception error)
            {
                _internalStream.OnError(error);
            }

            /// <summary>
            /// Called when [next].
            /// </summary>
            /// <param name="value">The value.</param>
            public void OnNext(T value)
            {
                _internalStream.OnNext(value);
            }

            #endregion IObserver Members
        }

        /// <summary>
        /// Subject monitor wrapper
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOunt">The type of the out.</typeparam>
        private class SubjectWrapper<TIn, TOunt> : ISubject<TIn, TOunt>
        {
            private ISubject<TIn, TOunt> _internalStream;
            private IObservable<TOunt> _monitorStream;

            #region Ctor

            public SubjectWrapper(IObservable<TOunt> monitorStream, ISubject<TIn, TOunt> internalStream)
            {
                _internalStream = internalStream;
                _monitorStream = monitorStream;
            }

            #endregion Ctor

            #region Subscribe

            /// <summary>
            /// Subscribes the specified observer.
            /// </summary>
            /// <param name="observer">The observer.</param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<TOunt> observer)
            {
                var callbackDisposable = new CancellationDisposable();
                IDisposable disp = _monitorStream
                    .Finally(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        })
                    .Subscribe(observer);
                callbackDisposable.Token.Register(() =>
                        {
                            _internalStream = null;
                            _monitorStream = null;
                        });
                return new CompositeDisposable(callbackDisposable, disp);
            }

            #endregion Subscribe

            #region IObserver Members

            /// <summary>
            /// Called when [completed].
            /// </summary>
            public void OnCompleted()
            {
                _internalStream.OnCompleted();
            }

            /// <summary>
            /// Called when [error].
            /// </summary>
            /// <param name="error">The error.</param>
            public void OnError(Exception error)
            {
                _internalStream.OnError(error);
            }

            /// <summary>
            /// Called when [next].
            /// </summary>
            /// <param name="value">The value.</param>
            public void OnNext(TIn value)
            {
                _internalStream.OnNext(value);
            }

            #endregion IObserver Members
        }

        #endregion SubjectWrapper

        #region DefaultSurrogate

        /// <summary>
        /// Default implementation of the surrogate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DefaultSurrogate<T>: IMonitorSurrogate<T>
        {
            #region SerializationStrategy

            /// <summary>
            /// Gets the serialization strategy.
            /// </summary>
            /// <value>
            /// The serialization strategy.
            /// </value>
            public MarbleSerializationOptions SerializationStrategy
            {
                get { return MarbleSerializationOptions.ToString; }
            }

            #endregion // SerializationStrategy

            #region Mapping

            /// <summary>
            /// Mappings the specified candidate.
            /// </summary>
            /// <param name="item"></param>
            /// <param name="candidate">The candidate.</param>
            /// <returns></returns>
            public object Mapping(T item, MarbleCandidate candidate)
            {
                return null; 
            }

            #endregion // Mapping
        }

        #endregion // DefaultSurrogate

        #endregion Nested Types
    }
}