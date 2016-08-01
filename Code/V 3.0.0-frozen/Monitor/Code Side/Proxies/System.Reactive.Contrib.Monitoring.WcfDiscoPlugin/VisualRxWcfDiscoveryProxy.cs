#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Threading;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Monitor proxy to Wcf discovery channel
    /// </summary>
    public class VisualRxWcfDiscoveryProxy : VisualRxProxyBase, IVisualRxFilterableProxy
    {
        #region Constants

        /// <summary>
        /// the VisualRxWcfDiscoveryProxy kind
        /// </summary>
        public const string KIND = "WcfDiscovery";

        private const string CHANNEL_TYPE_TCP = "net.tcp";
        private const string CHANNEL_TYPE_NAMEPIPE = "net.pipe";
        private const string NO_END_POINT_FOUND = "Discovery did not found any end point";

        #endregion Constants

        #region Private / Protected Fields

        private static ServiceFactory _service;

        #endregion Private / Protected Fields

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="VisualRxWcfDiscoveryProxy"/> class from being created.
        /// </summary>
        private VisualRxWcfDiscoveryProxy() { }

        #endregion // Ctor

        #region Properties

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind { get { return KIND; } }

        #endregion Kind

        #region Settings

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public VisualRxWcfDiscoverySettings Settings { get; private set; }

        #endregion Settings

        #endregion Properties

        #region Methods

        #region Create

        /// <summary>
        /// Creates proxy.
        /// </summary>
        /// <returns></returns>
        public static VisualRxWcfDiscoveryProxy Create(VisualRxWcfDiscoverySettings setting = null)
        {
            return new VisualRxWcfDiscoveryProxy { Settings = setting ?? new VisualRxWcfDiscoverySettings() };
        }

        #endregion Create

        #region OnInitialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public string OnInitialize()
        {
            try
            {
                _service = new ServiceFactory();
                string info = _service.Init(Settings);
                _service.Ping();
                return info;
            }

            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Warn("MonitorWcfDiscoveryProxy: ping faild, {0}", ex);
                return ex.ToString();
            }

            #endregion Exception Handling
        }

        #endregion OnInitialize

        #region OnBulkSend

        /// <summary>
        /// Bulk send.
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            try
            {
                _service.Send(items.ToArray());
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("MonitorWcfDiscoveryProxy: {0}", ex);
            }

            #endregion Exception Handling
        }

        #endregion OnBulkSend

        #endregion Methods

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
                _service.Dispose();
            }
            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("VisualRxTraceSourceProxy: {0}", ex);
            }
        }

        #endregion // Dispose

        #region ServiceFactory

        /// <summary>
        /// Handle the channel
        /// </summary>
        private class ServiceFactory : IDisposable
        {
            #region Private / Protected Fields

            private readonly object _sync = new object();
            private ConcurrentDictionary<string, ChannelInfo> _liveChannels = new ConcurrentDictionary<string, ChannelInfo>();
            private Timer _tmr;
            private ServiceHost _announcementServiceHost;
            private bool _isDiscovering = false;
            private VisualRxWcfDiscoverySettings _setting;

            #endregion Private / Protected Fields

            #region Init

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceFactory"/> class.
            /// </summary>
            public string Init(VisualRxWcfDiscoverySettings setting)
            {
                _setting = setting;
                HostAnnouncements();
                Discover();
                int duration = _setting.RediscoverIntervalMinutes * 60 * 1000;
                _tmr = new Timer(state => Discover(), null, duration, duration);

                if (_liveChannels.Any())
                {
                    var adresses = (from c in _liveChannels.Values
                                    select c.Channel.Endpoint.Address.Uri.AbsoluteUri);
                    return string.Join("\r\n", adresses);
                }
                else
                {
                    return NO_END_POINT_FOUND;
                }
            }

            #endregion Init

            #region Send

            /// <summary>
            /// Broadcast the specified items to all listeners
            /// </summary>
            /// <param name="items">The marble items.</param>
            public void Send(MarbleBase[] items)
            {
                //#region Validation

                //if (_liveChannels.Count == 0)
                //    Discover();

                //#endregion // Validation

                IEnumerable<ChannelInfo> channels = _liveChannels.Values;
                foreach (ChannelInfo channel in channels)
                {
                    // TODO: try catch and mark channel as faulted + fallback channel or dead queue
                    //channel.Proxy.BeginSend(items, null, null);
                    if (channel.Channel.State == CommunicationState.Opened)
                        channel.Proxy.Send(items);
                    else if (channel.Channel.State != CommunicationState.Opening)
                    {
                        Discover();
                        break;
                    }
                }
            }

            #endregion Send

            #region Ping

            /// <summary>
            /// Pings this instance.
            /// </summary>
            public void Ping()
            {
                try
                {
                    //#region Validation

                    //if (_liveChannels.Count == 0)
                    //    Discover();

                    //#endregion // Validation

                    IEnumerable<ChannelInfo> channels = _liveChannels.Values;
                    foreach (ChannelInfo channel in channels)
                    {
                        if (channel.Channel.State == CommunicationState.Opened)
                            channel.Proxy.Ping();
                        else if (channel.Channel.State != CommunicationState.Opening)
                        {
                            Discover();
                            break;
                        }
                    }
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Warn("Ping failed: {0}", ex);
                }

                #endregion Exception Handling
            }

            #endregion Ping

            #region Discover

            /// <summary>
            /// Discovers this listeners.
            /// </summary>
            private bool Discover()
            {
                lock (_sync)
                {
                    if (_isDiscovering)
                        return false;
                    _isDiscovering = true;
                }
                try
                {
                    var client = new DiscoveryClient(new UdpDiscoveryEndpoint());
                    
                    var criteria = new FindCriteria(typeof(IVisualRxServiceAdapter));
                    criteria.Duration = TimeSpan.FromSeconds(_setting.DiscoveryTimeoutSeconds);

                    var endpoints = client.Find(criteria).Endpoints;
                    var addressesNamepipe = (from item in endpoints
                                             where item.Address.Uri.Scheme == CHANNEL_TYPE_NAMEPIPE
                                             select item.Address).ToArray();
                    var addressesTcp = (from item in endpoints
                                        where item.Address.Uri.Scheme == CHANNEL_TYPE_TCP
                                        select item.Address).ToArray();

                    string[] oldProxies = _liveChannels.Keys.ToArray();

                    AddProxies(addressesTcp, () => new NetTcpBinding());
                    AddProxies(addressesNamepipe, () => new NetNamedPipeBinding());

                    string[] forCleanup = oldProxies.Except(_liveChannels.Keys).ToArray();

                    Cleanup(forCleanup);
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("Monitor channel discovery: ", ex);
                }

                #endregion Exception Handling

                finally
                {
                    lock (_sync)
                    {
                        _isDiscovering = false;
                    }
                }

                return true;
            }

            #endregion Discover

            #region AddProxies

            #region Overloads

            /// <summary>
            /// Adds the proxies from addresses.
            /// </summary>
            /// <param name="bindingFactory">The binding factory.</param>
            /// <param name="addresses">The addresses.</param>
            private void AddProxies(
                Func<Binding> bindingFactory,
                params EndpointAddress[] addresses)
            {
                AddProxies(addresses, bindingFactory);
            }

            #endregion Overloads

            /// <summary>
            /// Adds the proxies from addresses.
            /// </summary>
            /// <param name="addresses">The addresses.</param>
            /// <param name="bindingFactory">The binding factory.</param>
            private void AddProxies(
                IEnumerable<EndpointAddress> addresses,
                Func<Binding> bindingFactory)
            {
                foreach (var address in addresses)
                {
                    var channel = new ChannelFactory<IVisualRxServiceAdapterAsync>(
                        bindingFactory(), address);
                    channel.Faulted += (s, e) => Discover();
                    _liveChannels.TryAdd(address.Uri.AbsolutePath, new ChannelInfo(channel));
                }
            }

            #endregion AddProxies

            #region Announcements

            #region HostAnnouncements

            /// <summary>
            /// Hosts the announcements listener.
            /// </summary>
            private void HostAnnouncements()
            {
                // TODO: 
                // host.AddServiceEndpoint(new AnnouncementEndpoint(new NetTcpBinding(), new EndpointAddress("net.tcp://127.0.0.1")));

                // Create an AnnouncementService instance
                AnnouncementService announcementService = new AnnouncementService();

                // Subscribe the announcement events
                announcementService.OnlineAnnouncementReceived += OnOnlineAnnoncement;    // hay
                announcementService.OfflineAnnouncementReceived += OnOfflineAnnoncement;  // bay

                // Create ServiceHost for the AnnouncementService
                _announcementServiceHost = new ServiceHost(announcementService);
                // TODO: add reduction to IVisualRxServiceAdapter contract !!!

                // Listen for the announcements sent over UDP multicast
                var endpoint = new UdpAnnouncementEndpoint();
                _announcementServiceHost.AddServiceEndpoint(endpoint);
                //_announcementServiceHost.BeginOpen(ar => { }, null);
            }

            #endregion HostAnnouncements

            #region OnOnlineAnnoncement

            /// <summary>
            /// Called when [online annoncement].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.ServiceModel.Discovery.AnnouncementEventArgs"/> instance containing the event data.</param>
            private void OnOnlineAnnoncement(object sender, AnnouncementEventArgs e)
            {
                var address = e.EndpointDiscoveryMetadata.Address;

                Uri uri = address.Uri;
                if (uri.AbsoluteUri.EndsWith("mex"))
                    return;
                if (uri.Scheme == CHANNEL_TYPE_NAMEPIPE)
                    AddProxies(() => new NetNamedPipeBinding(), address);
                else if (uri.Scheme != CHANNEL_TYPE_NAMEPIPE)
                    AddProxies(() => new NetTcpBinding(), address);

                try
                {
                    Ping();
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Warn("MonitorWcfDiscoveryProxy: ping faild, {0}", ex);
                }

                #endregion Exception Handling
            }

            #endregion OnOnlineAnnoncement

            #region OnOfflineAnnoncement

            /// <summary>
            /// Called when [offline annoncement].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.ServiceModel.Discovery.AnnouncementEventArgs"/> instance containing the event data.</param>
            private void OnOfflineAnnoncement(object sender, AnnouncementEventArgs e)
            {
                var address = e.EndpointDiscoveryMetadata.Address;

                Uri uri = address.Uri;
                if (uri.AbsoluteUri.EndsWith("mex"))
                    return;

                Cleanup(uri.AbsolutePath);
            }

            #endregion OnOfflineAnnoncement

            #endregion Announcements

            #region Cleanup

            /// <summary>
            /// Cleanups the specified old channels.
            /// </summary>
            /// <param name="oldChannelKeys">The old channels.</param>
            private void Cleanup(params string[] oldChannelKeys)
            {
                foreach (var channelKey in oldChannelKeys)
                {
                    ChannelInfo channelInfo;
                    var exists = _liveChannels.TryRemove(channelKey, out channelInfo);

                    if (!exists)
                        continue;

                    try
                    {
                        var channel = channelInfo.Channel;
                        switch (channel.State)
                        {
                            case CommunicationState.Faulted:
                                channel.Abort();
                                break;

                            case CommunicationState.Created:
                            case CommunicationState.Opened:
                            case CommunicationState.Opening:
                                channel.Close();
                                break;
                        }
                    }

                    #region Exception Handling

                    catch (Exception ex)
                    {
                        TraceSourceMonitorHelper.Error("Monitor channel disposal: ", ex);
                    }

                    #endregion Exception Handling
                }
            }

            #endregion Cleanup

            #region Channel

            /// <summary>
            /// Encapsulate the proxy and the channel together
            /// </summary>
            private class ChannelInfo
            {
                public ChannelInfo(ChannelFactory<IVisualRxServiceAdapterAsync> channel)
                {
                    Channel = channel;
                    Proxy = channel.CreateChannel();
                }

                public ChannelFactory<IVisualRxServiceAdapterAsync> Channel { get; set; }

                public IVisualRxServiceAdapterAsync Proxy { get; set; }
            }

            #endregion Channel

            #region Dispose Pattern

            /// <summary>
            /// Releases unmanaged resources and performs other cleanup operations before the
            /// <see cref="ServiceFactory"/> is reclaimed by garbage collection.
            /// </summary>
            ~ServiceFactory()
            {
                Dispose(false);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
            private void Dispose(bool disposed)
            {
                try
                {
                    _tmr.Dispose();
                    Cleanup(_liveChannels.Keys.ToArray());

                    switch (_announcementServiceHost.State)
                    {
                        case CommunicationState.Faulted:
                            _announcementServiceHost.Abort();
                            break;

                        case CommunicationState.Created:
                        case CommunicationState.Opened:
                        case CommunicationState.Opening:
                            _announcementServiceHost.Close();
                            break;
                    }
                }
                catch (Exception) { }
            }

            #endregion Dispose Pattern
        }

        #endregion ServiceFactory
    }
}