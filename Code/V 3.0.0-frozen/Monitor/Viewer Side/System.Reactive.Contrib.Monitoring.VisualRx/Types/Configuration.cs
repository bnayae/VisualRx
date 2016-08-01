using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace System.Reactive.Contrib.Monitoring.UI
{
    [DataContract]
    public class Configuration
    {
        private const string DEF_PLUGIN_FOLDER = "Plugins";

        #region Ctor

        public Configuration()
        {
            PluginDiscoveryPaths = new ObservableCollection<DiscoveryPath> 
            {
                new DiscoveryPath{ Path = DEF_PLUGIN_FOLDER}
            };
        }

        #endregion // Ctor

        #region PluginDiscoveryPaths

        /// <summary>
        /// Gets or sets the plug-in discovery paths.
        /// </summary>
        /// <value>
        /// The plug-in discovery paths.
        /// </value>
        [DataMember]
        public ObservableCollection<DiscoveryPath> PluginDiscoveryPaths { get; private set; }

        #endregion // PluginDiscoveryPaths

        #region PluginsActivation

        /// <summary>
        /// Gets or sets the plug-ins activation.
        /// </summary>
        /// <value>
        /// The plug-ins activation.
        /// </value>
        [DataMember]
        public ConcurrentDictionary<Guid, bool> PluginsActivation { get; set; }

        #endregion // PluginsActivation

        #region Interception Points

        #region // OnSerializingMethod

        /// <summary>
        /// Called when [serializing method].
        /// </summary>
        /// <param name="context">The context.</param>
        //[OnSerializing()]
        //private void OnSerializingMethod(StreamingContext context)
        //{
        //}

        #endregion // OnSerializingMethod

        #region // OnSerializedMethod

        ///// <summary>
        ///// Called when [serialized method].
        ///// </summary>
        ///// <param name="context">The context.</param>
        //[OnSerialized()]
        //private void OnSerializedMethod(StreamingContext context)
        //{
        //}

        #endregion // OnSerializedMethod

        #region // OnDeserializingMethod

        ///// <summary>
        ///// Called when [deserializing method].
        ///// </summary>
        ///// <param name="context">The context.</param>
        //[OnDeserializing()]
        //private void OnDeserializingMethod(StreamingContext context)
        //{
        //}

        #endregion // OnDeserializingMethod

        #region OnDeserializedMethod

        /// <summary>
        /// Called when [deserialized method].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (PluginDiscoveryPaths == null || !PluginDiscoveryPaths.Any())
            {
                PluginDiscoveryPaths = new ObservableCollection<DiscoveryPath> 
                {
                    new DiscoveryPath{ Path = DEF_PLUGIN_FOLDER }
                };
            }
        }

        #endregion // OnDeserializedMethod

        #endregion // Interception Points
    }
}