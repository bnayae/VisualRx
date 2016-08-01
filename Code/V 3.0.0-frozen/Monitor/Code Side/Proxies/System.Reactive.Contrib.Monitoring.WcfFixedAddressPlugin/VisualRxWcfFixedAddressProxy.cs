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

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Send monitor info through WCF channel
    /// </summary>
    public class VisualRxWcfFixedAddressProxy : VisualRxProxyBase, IVisualRxFilterableProxy
    {
        #region Constants

        /// <summary>
        /// the kind of VisualRxWcfFixedAddressProxy
        /// </summary>
        public const string KIND = "WcfFixedAddressProxy";
        private const string DEFAULT_HTTP_FORMAT = "http://{0}:6017/VisualRx";
        private static readonly TimeSpan DEFAULT_CHANNEL_RECOVERY_THROTTELING = TimeSpan.FromSeconds(10);

        #endregion Constants

        #region Private / Protected Fields

        private readonly Binding _binding;
        private readonly EndpointAddress _endpoint;
        private readonly Lazy<ProxyFactory> _proxy;
        private readonly TimeSpan _channelRecoveryThrottling;

        #endregion Private / Protected Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualRxWcfFixedAddressProxy"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="channelRecoveryThrottling">The minimal duration between channel renew (end point channel recovery)</param>
        private VisualRxWcfFixedAddressProxy(Binding binding, EndpointAddress endpoint, TimeSpan channelRecoveryThrottling)
        {
            _binding = binding;
            _endpoint = endpoint;
            TimeSpan recoveryThrottling = channelRecoveryThrottling;
            _proxy = new Lazy<ProxyFactory>(() => new ProxyFactory(binding, endpoint, recoveryThrottling));
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

        #region CreateDefaultHttp

        /// <summary>
        /// Creates the default HTTP.
        /// </summary>
        /// <param name="host">Host name or IP.</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy CreateDefaultHttp(string host)
        {
            return CreateDefaultHttp(host, DEFAULT_CHANNEL_RECOVERY_THROTTELING);
        }

        /// <summary>
        /// Creates the default HTTP.
        /// </summary>
        /// <param name="host">Host name or IP.</param>
        /// <param name="channelRecoveryThrottling">The minimal duration between channel renew (end point channel recovery)</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy CreateDefaultHttp(string host, TimeSpan channelRecoveryThrottling)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            string uri = string.Format(DEFAULT_HTTP_FORMAT, host);
            var endpoint = new EndpointAddress(uri);
            return new VisualRxWcfFixedAddressProxy(binding, endpoint, channelRecoveryThrottling);
        }

        #endregion // CreateDefaultHttp

        #region Create

        #region Overloads

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy Create(Binding binding, string uri)
        {
            return Create(binding, uri, DEFAULT_CHANNEL_RECOVERY_THROTTELING);
        }
        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="channelRecoveryThrottling">The minimal duration between channel renew (end point channel recovery)</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy Create(Binding binding, string uri, TimeSpan channelRecoveryThrottling)
        {
            var endpoint = new EndpointAddress(uri);
            return Create(binding, endpoint, channelRecoveryThrottling);
        }

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="channelRecoveryThrottling">The minimal duration between channel renew (end point channel recovery)</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy Create(Binding binding, EndpointAddress endpoint)
        {
            return Create(binding, endpoint, DEFAULT_CHANNEL_RECOVERY_THROTTELING);
        }

        #endregion // Overloads

        /// <summary>
        /// Creates the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="channelRecoveryThrottling">The minimal duration between channel renew (end point channel recovery)</param>
        /// <returns></returns>
        public static VisualRxWcfFixedAddressProxy Create(
            Binding binding, 
            EndpointAddress endpoint, 
            TimeSpan channelRecoveryThrottling)
        {
            return new VisualRxWcfFixedAddressProxy(binding, endpoint, channelRecoveryThrottling);
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
                TraceSourceMonitorHelper.Error("VisualRxWcfFixedAddressProxy: {0}", ex);
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
            private DateTime _lastChannelRecovery = DateTime.MinValue;
            private readonly TimeSpan _channelRecoveryThrottling;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ProxyFactory"/> class.
            /// </summary>
            /// <param name="binding">The binding.</param>
            /// <param name="address">The address.</param>
            public ProxyFactory(Binding binding, EndpointAddress address, TimeSpan channelRecoveryThrottling)
            {
                _binding = binding;
                _address = address;
                _channelRecoveryThrottling = channelRecoveryThrottling;
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
                    if (proxy == null)
                        return;
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
                    try
                    {
                        if (Interlocked.CompareExchange(ref _factory, null, factory) == factory)
                            factory.Abort(); // abort once
                    }
                    catch { }
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
                // The sending is synchronized at the infrastructure level 
                // therefore this class shouldn't be thread-safe

                #region Validation

                if (factory != null && factory.State == CommunicationState.Opening)
	            {
                    SpinWait.SpinUntil(() =>_factory.State != CommunicationState.Opening, 1000);
	            }
                if (factory == null || factory.State != CommunicationState.Opened)
                {
                    if (factory != null && 
                        factory.State != CommunicationState.Closed &&
                        factory.State != CommunicationState.Closing)
                    {
                        try
                        {
                            factory.Abort();
                        }
                        catch { }
                    }

                    if (_lastChannelRecovery.Add(_channelRecoveryThrottling) > DateTime.UtcNow)
                        return null;

                    TraceSourceMonitorHelper.Debug("Recovering channel for [{0}]", _address.Uri);

                    factory = new ChannelFactory<IVisualRxServiceAdapterAsync>(_binding, _address);
                    _proxyCache = factory.CreateChannel();
                    _factory = factory;

                    _lastChannelRecovery = DateTime.UtcNow;
                }

                #endregion // Validation

                return _proxyCache;
            }

            #endregion // GetProxy

            #region Dispose

            public void Dispose()
            {
                var factory = _factory;
                if (factory == null)
                    return;
                if (Interlocked.CompareExchange(ref _factory, null, factory) != factory)
                    return;

                try
                {
                    if (factory.State == CommunicationState.Faulted)
                        factory.Abort();
                    else
                        factory.Close();
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