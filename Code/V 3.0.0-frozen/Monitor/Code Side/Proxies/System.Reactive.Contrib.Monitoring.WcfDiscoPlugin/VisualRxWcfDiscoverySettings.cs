#region Using

using System;
using System.Diagnostics;
using System.Linq;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Wcf discovery proxy's setting
    /// </summary>
    public class VisualRxWcfDiscoverySettings
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualRxWcfDiscoverySettings"/> class.
        /// </summary>
        public VisualRxWcfDiscoverySettings()
        {
            DiscoveryTimeoutSeconds = 3;
            RediscoverIntervalMinutes = 30;
        }

        #endregion Ctor

        #region DiscoveryTimeoutSeconds

        /// <summary>
        /// Gets or sets the discovery timeout seconds.
        /// Wcf discovery limit the duration where it waiting for endpoint response.
        /// </summary>
        /// <value>
        /// The discovery timeout seconds.
        /// </value>
        public int DiscoveryTimeoutSeconds { get; set; }

        #endregion DiscoveryTimeoutSeconds

        #region RediscoverIntervalMinutes

        /// <summary>
        /// Gets or sets the rediscover interval minutes.
        /// on elapse the proxy will retry actively to re-discover endpoints
        /// (remember that endpoints are also announcing when they are ready, therefore
        /// the rediscover is just a backup in case that the proxy is missing the announcement).
        /// </summary>
        /// <value>
        /// The rediscover interval minutes.
        /// </value>
        public int RediscoverIntervalMinutes { get; set; }

        #endregion RediscoverIntervalMinutes
    }
}