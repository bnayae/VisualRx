#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Reflection;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Base class for plug-in 
    /// </summary>
    public abstract class PluginBundleBase : IPluginBundle
    {
        private static readonly Version DEFAULT_VERSION =
            Assembly.GetExecutingAssembly().GetName().Version;

        #region Id

        /// <summary>
        /// Gets the id.
        /// </summary>
        public abstract Guid Id { get; }

        #endregion // Id

        #region Title

        /// <summary>
        /// Gets the title.
        /// </summary>
        public abstract string Title { get; }

        #endregion // Title

        #region Description

        /// <summary>
        /// Gets the description.
        /// </summary>
        public abstract string Description { get; }

        #endregion // Description

        #region Icon

        /// <summary>
        /// Gets the plug-in's icon.
        /// </summary>
        public virtual ImageSource Icon
        {
            get { return null; }
        }

        #endregion // Icon

        #region Version

        /// <summary>
        /// Gets the version.
        /// </summary>
        public virtual Version Version { get { return DEFAULT_VERSION; } }

        #endregion // Version

        #region PublisherName

        /// <summary>
        /// Gets the publisher.
        /// </summary>
        public abstract string PublisherName { get; }

        #endregion // PublisherName

        #region PublisherEmail

        /// <summary>
        /// Gets the publisher email.
        /// </summary>
        /// <value>
        /// The publisher email.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual string PublisherEmail
        {
            get { return string.Empty; }
        }

        #endregion // PublisherEmail

        #region ProductUrl

        /// <summary>
        /// Gets the plug-in url.
        /// </summary>
        public virtual string ProductUrl { get { return string.Empty; } }

        #endregion // ProductUrl

        #region SupportUrl

        /// <summary>
        /// Gets the plug-in support URL.
        /// </summary>
        public virtual string SupportUrl { get { return string.Empty; } }

        #endregion // SupportUrl

        #region HasCustomConfiguration

        /// <summary>
        /// Gets a value indicating whether this plug--in is having a custom configuration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has custom configuration; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasCustomConfiguration { get { return false; } }

        #endregion // HasCustomConfiguration

        #region EditConfiguration

        /// <summary>
        /// Edits the configuration (also responsible to handle the configuration persistence).
        /// </summary>
        public virtual void EditConfiguration() { }

        #endregion // EditConfiguration

        #region LoadResourcesPlugin

        /// <summary>
        /// Gets the load resources plug-in.
        /// use this plug-in in order to load additional resources.
        /// </summary>
        public virtual ILoadResourcesPlugin LoadResourcesPlugin
        {
            get { return null; }
        }

        #endregion // LoadResourcesPlugin

        #region InterceptionPlugin

        /// <summary>
        /// Gets the interception plug-in.
        /// use this plug-in for non-UI interception
        /// </summary>
        public virtual IItemInterceptionPlugin InterceptionPlugin
        {
            get { return null; }
        }

        #endregion // InterceptionPlugin

        #region LineHeaderImagePlugin

        /// <summary>
        /// Gets the diagram image mapping plug-ins.
        /// used to replace the diagram's image.
        /// </summary>
        public virtual ILineHeaderImagePlugin LineHeaderImagePlugin
        {
            get { return null; }
        }

        #endregion // LineHeaderImagePlugin

        #region MarblePanelPlugin

        /// <summary>
        /// Gets the marble diagram panel template selector plug-in.
        /// use this option in order to replace or manipulate the the marble-diagram template.
        /// </summary>
        public virtual IMarblePanelPlugin MarblePanelPlugin
        {
            get { return null; }
        }

        #endregion // MarblePanelPlugin

        #region MarbleItemPlugin

        /// <summary>
        /// Gets the marble item template plug-in.
        /// use this option in order to replace a marble template. 
        /// </summary>
        public virtual IMarbleItemPlugin MarbleItemPlugin
        {
            get { return null; }
        }

        #endregion // MarbleTemplatePlugin

        #region MarbleItemPlugins

        /// <summary>
        /// Gets the marble item template plug-ins.
        /// use this option in order to replace a marble template. 
        /// </summary>
        public virtual IEnumerable<IMarbleItemPlugin> MarbleItemPlugins
        {
            get { return Enumerable.Empty<IMarbleItemPlugin>(); }
        }

        #endregion // MarbleTemplatePlugins

        #region TabTemplatePlugin

        /// <summary>
        /// Gets the tab template plug-in.
        /// use this plug-in in order of adding a new tabs.
        /// </summary>
        public virtual ITabPlugin TabPlugin
        {
            get { return null; }
        }

        #endregion // TabTemplatePlugin

    }
}