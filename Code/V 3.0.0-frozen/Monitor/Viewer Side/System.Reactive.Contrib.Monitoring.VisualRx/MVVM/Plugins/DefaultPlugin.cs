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

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Grid Tab plug-in 
    /// </summary>
    public class DefaultPlugin : PluginBundleBase
    {
        private static readonly Guid ID = new Guid("18945976-A120-4761-91A5-7EC1625024CD");
        private const string TITLE = "Default Plug-ins";
        private const string DESC = "the default Visual Rx plug-ins";

        private Lazy<ITabPlugin> _tabsPlugin =
            new Lazy<ITabPlugin>(() => new MainTabPlugin());

        #region Id

        /// <summary>
        /// Gets the id.
        /// </summary>
        public override Guid Id { get { return ID; } }

        #endregion // Id

        #region Title

        /// <summary>
        /// Gets the title.
        /// </summary>
        public override string Title { get { return TITLE; } }

        #endregion // Title

        #region Description

        /// <summary>
        /// Gets the description.
        /// </summary>
        public override string Description { get { return DESC; } }

        #endregion // Description

        #region Publisher

        /// <summary>
        /// Gets the publisher.
        /// </summary>
        public override string PublisherName
        {
            get { return "Bnaya Eshet"; }
        }

        #endregion // Publisher

        #region Url

        /// <summary>
        /// Gets the plug-in url.
        /// </summary>
        public override string ProductUrl
        {
            get
            {
                return "http://blogs.microsoft.co.il/blogs/bnaya/archive/2012/08/12/visual-rx-toc.aspx";
            }
        }

        #endregion // Url

        #region Icon

        ///// <summary>
        ///// Gets the plug-in's icon.
        ///// </summary>
        //public override ImageSource Icon
        //{
        //    get { return null; }
        //}

        #endregion // Icon

        #region TabTemplatePlugin

        /// <summary>
        /// Gets the tab template plug-in.
        /// use this plug-in in order of adding a new tabs.
        /// </summary>
        public override ITabPlugin TabPlugin
        {
            get { return _tabsPlugin.Value; }
        }

        #endregion // TabTemplatePlugin
    }
}