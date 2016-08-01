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
    /// Tabular Tab plug-in 
    /// </summary>
    public class TabularTabPlugin : PluginBundleBase
    {
        private static readonly Guid ID = new Guid("148053EF-F552-4132-950E-7ADEA130FCC5");
        private const string TITLE = "Tabular Tab";
        private const string DESC = "Add a tab which present the stream in a tabular format.";

        private Lazy<ITabPlugin> _gridTabPlugin =
            new Lazy<ITabPlugin>(() => new GridTabPlugin());
        private Lazy<ILoadResourcesPlugin> _loadResourcesPlugin =
            new Lazy<ILoadResourcesPlugin>(() => new LoadResourcesPlugin());

        private readonly Uri HOT_IMG = UIPluginHelper.GetPackResourceUri("Images/Dolar.png");

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

        #region LoadResourcesPlugin

        /// <summary>
        /// Gets the load resources plug-ins.
        /// use this plug-in in order to load additional resources.
        /// </summary>
        public override ILoadResourcesPlugin LoadResourcesPlugin
        {
            get { return _loadResourcesPlugin.Value; }
        }

        #endregion // LoadResourcesPlugin

        #region TabTemplatePlugin

        /// <summary>
        /// Gets the tab template plug-ins.
        /// use this plug-in in order of adding a new tabs.
        /// </summary>
        public override ITabPlugin TabPlugin
        {
            get { return _gridTabPlugin.Value; }
        }

        #endregion // TabTemplatePlugin
    }
}