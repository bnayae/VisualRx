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

namespace System.Reactive.Contrib.Monitoring.SamplePlugin
{
    /// <summary>
    /// Sample plug-in 
    /// </summary>
    public class Plugin : PluginBundleBase
    {
        private static readonly Guid ID = new Guid("69BFD73B-728F-4789-924B-0540BDE4EF3E");
        private const string TITLE = "Sample Plug-ins";
        private const string DESC = "Sample plug-ins which demonstrate different Visual Rx plug-in techniques";

        private readonly Uri HOT_IMG = UIPluginHelper.GetPackResourceUri("Images/hot.png");

        private Lazy<ILoadResourcesPlugin> _loadResourcesPlugin =
            new Lazy<ILoadResourcesPlugin>(() => new LoadResourcesPlugin());
        private Lazy<ILineHeaderImagePlugin> _marbleImagePlugin =
            new Lazy<ILineHeaderImagePlugin>(() => new ImageMapperPlugin());
        private Lazy<IMarbleItemPlugin> _marbleTemplatePlugin =
            new Lazy<IMarbleItemPlugin>(() => new PseudoTemplateSelectorPlugin());

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

        #region LineHeaderImagePlugin

        /// <summary>
        /// Gets the diagram image mapping plug-in.
        /// used to replace the diagram's image.
        /// </summary>
        public override ILineHeaderImagePlugin LineHeaderImagePlugin
        {
            get
            {
                return _marbleImagePlugin.Value;
            }
        }

        #endregion // LineHeaderImagePlugin

        #region MarbleItemPlugin

        /// <summary>
        /// Gets the marble template plug-in.
        /// use this option in order to replace a marble template.
        /// </summary>
        public override IMarbleItemPlugin MarbleItemPlugin
        {
            get
            {
                return _marbleTemplatePlugin.Value;
            }
        }

        #endregion // MarbleItemPlugin
    }
}