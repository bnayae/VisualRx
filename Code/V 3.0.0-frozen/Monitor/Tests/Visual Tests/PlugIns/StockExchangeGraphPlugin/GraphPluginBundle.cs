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
using System.Windows.Media.Imaging;

#endregion Using

namespace Bnaya.Samples
{
    /// <summary>
    /// Sample plug-in 
    /// </summary>
    public class GraphPluginBundle : PluginBundleBase
    {
        private static readonly Guid ID = new Guid("2C816F32-F32B-4136-A9C1-9E66FA9F4D40");
        private const string TITLE = "Stock Exchange Graph";
        private const string DESC = "Format the Stock Exchange datum as a graph.";

        private Lazy<GraphPlugin> _pluginImplementation =
            new Lazy<GraphPlugin>(() => new GraphPlugin());

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

        /// <summary>
        /// Gets the plug-in's icon.
        /// </summary>
        public override ImageSource Icon
        {
            get { return new BitmapImage(HOT_IMG); }
        }

        #endregion // Icon

        #region LoadResourcesPlugin

        /// <summary>
        /// Gets the load resources plug-in.
        /// use this plug-in in order to load additional resources.
        /// </summary>
        public override ILoadResourcesPlugin LoadResourcesPlugin
        {
            get { return _pluginImplementation.Value; }
        }

        #endregion // LoadResourcesPlugin

        #region MarbleDiagramTemplatePlugin

        /// <summary>
        /// Gets the marble diagram template selector plug-in.
        /// use this option in order to replace or manipulate the the marble-diagram template.
        /// </summary>
        public override IMarblePanelPlugin MarblePanelPlugin
        {
            get
            {
                return _pluginImplementation.Value;
            }
        }

        #endregion // MarbleDiagramTemplatePlugin
    }
}