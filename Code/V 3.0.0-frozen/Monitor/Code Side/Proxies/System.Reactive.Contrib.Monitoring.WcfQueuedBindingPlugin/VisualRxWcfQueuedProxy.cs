#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.Security;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;

#endregion Using

// TODO: LIMIT the QueueSubject buffer size
// TODO: handle channel failure

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Send monitor info through WCF queued channel
    /// </summary>
    public class VisualRxWcfQueuedProxy : VisualRxProxyBase, IVisualRxFilterableProxy
    {
        #region Constants

        /// <summary>
        /// the kind of VisualRxWcfQueuedProxy
        /// </summary>
        public const string KIND = "WcfQueued";
        /// <summary>
        /// Default Queue Path
        /// </summary>
        public const string DefaultQueuePath = "net.msmq://localhost/private/VisualRx";

        #endregion Constants

        #region Private / Protected Fields

        private readonly NetMsmqBinding _binding;
        private readonly EndpointAddress _endpoint;
        private readonly Lazy<ProxyFactory> _proxy;

        #endregion Private / Protected Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualRxWcfQueuedProxy"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        private VisualRxWcfQueuedProxy(NetMsmqBinding binding, EndpointAddress endpoint)
        {
            _binding = binding;
            _endpoint = endpoint;
            _proxy = new Lazy<ProxyFactory>(() => new ProxyFactory(binding, endpoint));
        }

        #endregion // Ctor

        #region Properties

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind { get { return KIND; } }

        #endregion Kind

        #endregion Properties

        #region Methods

        #region OnBulkSend

        /// <summary>
        /// Send a balk.
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            _proxy.Value.Send(items.ToArray());
        }

        #endregion OnBulkSend

        #region OnInitialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public string OnInitialize()
        {
            _proxy.Value.Ping();
            return _proxy.Value.Endpoint;
        }

        #endregion OnInitialize

        #endregion Methods

        #region Create

        #region Overloads

        /// <summary>
        /// Creates proxy.
        /// </summary>
        /// <returns></returns>
        /// <remarks>the default message's time to live is 30 minutes</remarks>
        public static VisualRxWcfQueuedProxy Create()
        {
            return Create(TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// Creates the specified time to live.
        /// </summary>
        /// <param name="timeToLive">The time to live.</param>
        /// <returns></returns>
        public static VisualRxWcfQueuedProxy Create(TimeSpan timeToLive)
        {
            var binding = new NetMsmqBinding (NetMsmqSecurityMode.None)
            {
                ExactlyOnce = false,
                Durable = false,
                TimeToLive = timeToLive
            };
            return Create(binding, DefaultQueuePath);
        }

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns></returns>
        public static VisualRxWcfQueuedProxy Create(NetMsmqBinding binding)
        {
            var endpoint = new EndpointAddress(DefaultQueuePath);
            return Create(binding, endpoint);
        }

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="queuePath">The queue path.</param>
        /// <returns></returns>
        public static VisualRxWcfQueuedProxy Create(NetMsmqBinding binding, string queuePath)
        {
            var endpoint = new EndpointAddress(queuePath);
            return Create(binding, endpoint);
        }

        #endregion // Overloads

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns></returns>
        public static VisualRxWcfQueuedProxy Create(NetMsmqBinding binding, EndpointAddress endpoint)
        {
            return new VisualRxWcfQueuedProxy(binding, endpoint);
        }

        #endregion Create

        #region Dispose

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [SecuritySafeCritical]
        protected override void Dispose(bool disposed)
        {
            try
            {
                _proxy.Value.Dispose();
            }
            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("VisualRxWcfQueuedProxy: {0}", ex);
            }
        }

        #endregion // Dispose

        #region ProxyFactory

        /// <summary>
        /// Proxy
        /// </summary>
        private class ProxyFactory : IVisualRxServiceAdapter, IDisposable
        {
            private ChannelFactory<IVisualRxServiceAdapterAsync> _factory;
            private IVisualRxServiceAdapterAsync _proxyCache;
            private Binding _binding;
            private EndpointAddress _address;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyFactory"/> class.
            /// </summary>
            /// <param name="binding">The binding.</param>
            /// <param name="address">The address.</param>
            public ProxyFactory(Binding binding, EndpointAddress address)
            {
                _binding = binding;
                _address = address;
                _factory = new ChannelFactory<IVisualRxServiceAdapterAsync>(binding, address);
            }

            #endregion // Ctor

            #region Send

            /// <summary>
            /// Sends the specified item.
            /// </summary>
            /// <param name="item">The item.</param>
            public async void Send(MarbleBase[] item)
            {
                var factory = _factory;
                try
                {
                    var proxy = GetProxy(factory);
                    await proxy.SendAsync(item);
                }
                #region Exception Handling

                //catch (CommunicationException ex)
                //{
                //    factory.Abort();
                //    TraceSourceMonitorHelper.Error("Send CommunicationException: {0}", ex);
                //}
                //catch (TimeoutException ex)
                //{
                //    factory.Abort();
                //    TraceSourceMonitorHelper.Error("Send timeout: {0}", ex);
                //}
                catch (Exception ex)
                {
                    //factory.Abort();
                    TraceSourceMonitorHelper.Error("Send failed: {0}", ex);
                }

                #endregion // Exception Handling
            }

            #endregion // Send

            #region Ping

            /// <summary>
            /// Pings this instance.
            /// </summary>
            public async void Ping()
            {
                var factory = _factory;
                try
                {
                    var proxy = GetProxy(factory);
                    await proxy.PingAsync();
                }
                #region Exception Handling

                //catch (CommunicationException ex)
                //{
                //    factory.Abort();
                //    TraceSourceMonitorHelper.Error("Ping CommunicationException: {0}", ex);
                //}
                //catch (TimeoutException ex)
                //{
                //    factory.Abort();
                //    TraceSourceMonitorHelper.Error("Ping timeout: {0}", ex);
                //}
                catch (Exception ex)
                {
                    //factory.Abort();
                    TraceSourceMonitorHelper.Error("Ping failed: {0}", ex);
                }

                #endregion // Exception Handling
            }

            #endregion // Ping

            #region Endpoint

            /// <summary>
            /// Gets the endpoint.
            /// </summary>
            /// <value>
            /// The endpoint.
            /// </value>
            public string Endpoint
            {
                get
                {
                    return _address.Uri.AbsoluteUri;
                }
            }

            #endregion // Endpoint

            #region GetProxy

            /// <summary>
            /// Gets the proxy.
            /// </summary>
            /// <returns></returns>
            private IVisualRxServiceAdapterAsync GetProxy(ChannelFactory<IVisualRxServiceAdapterAsync> factory)
            {
                #region Validation

                if (factory.State == CommunicationState.Opening)
                {
                    SpinWait.SpinUntil(() => _factory.State != CommunicationState.Opening, 1000);
                }
                if (factory.State != CommunicationState.Opened)
                {
                    if (factory.State != CommunicationState.Closed &&
                        factory.State != CommunicationState.Closing)
                    {
                        try
                        {
                            factory.Abort();
                        }
                        catch { }
                    }

                    factory = new ChannelFactory<IVisualRxServiceAdapterAsync>(_binding, _address);
                    _proxyCache = factory.CreateChannel();
                    _factory = factory;
                }

                #endregion // Validation

                return _proxyCache;
            }

            #endregion // GetProxy

            #region Dispose

            public void Dispose()
            {
                try
                {
                    _factory.Close();
                }
                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("Dispose: close factory failed: {0}", ex);
                }
                GC.SuppressFinalize(this);
            }

            ~ProxyFactory()
            {
                Dispose();
            }

            #endregion // Dispose
        }

        #endregion ProxyFactory

    }
}