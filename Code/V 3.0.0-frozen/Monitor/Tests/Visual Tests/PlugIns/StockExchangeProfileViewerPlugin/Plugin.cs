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
    public class Plugin : PluginBundleBase
    {
        private static readonly Guid ID = new Guid("28CB56B9-ECFD-43AC-BABD-19E56B4A226B");
        private const string TITLE = "Stock Exchange Sample";
        private const string DESC = "Format the Stock Exchange datum.";

        private Lazy<IMarbleItemPlugin> _templatePlugin =
            new Lazy<IMarbleItemPlugin>(() => new MarbleTemplateSelectorPlugin());
        private Lazy<IMarblePanelPlugin> _templateDiagramPlugin =
            new Lazy<IMarblePanelPlugin>(() => new MarbleDiagramTemplateSelectorPlugin());
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
            get { return _loadResourcesPlugin.Value; }
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
                return _templateDiagramPlugin.Value;
            }
        }

        #endregion // MarbleDiagramTemplatePlugin

        #region MarbleTemplatePlugin

        /// <summary>
        /// Gets the marble template plug-in.
        /// use this option in order to replace a marble template.
        /// </summary>
        public override IMarbleItemPlugin MarbleItemPlugin
        {
            get
            {
                return _templatePlugin.Value;
            }
        }

        #endregion // MarbleTemplatePlugin
    }
}