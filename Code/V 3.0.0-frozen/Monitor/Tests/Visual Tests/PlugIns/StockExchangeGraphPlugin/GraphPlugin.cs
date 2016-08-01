#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows;

#endregion Using

namespace Bnaya.Samples
{
    /// <summary>
    /// Plug-in implementation
    /// </summary>
    public class GraphPlugin : ILoadResourcesPlugin, IMarblePanelPlugin
    {
        private readonly string ASSEMBLY_NAME = UIPluginHelper.GetAssemblyName();

        #region ILoadResourcesPlugin Implementation

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns></returns>
        public ResourceDictionary[] GetResources()
        {
            Uri uri = UIPluginHelper.GetResourceUri(ASSEMBLY_NAME, "StockExchangeResource.xaml");
            ResourceDictionary myResource = new ResourceDictionary();
            myResource.Source = uri;
            return new ResourceDictionary[] { myResource };
        }

        #endregion // ILoadResourcesPlugin Implementation

        #region IMarblePanelPlugin implementation

        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="item">The diagram.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public DataTemplate SelectTemplate(IMarbleDiagramContext marbleDiagram, FrameworkElement element)
        {
            DataTemplate template = null;
            if (marbleDiagram.Name.StartsWith("StockGraph"))
            {
                template = element.FindResource("StockDiagramGraphTemplate") as DataTemplate;
            }
            return template;
        }

        #endregion IMarblePanelPlugin implementation
    }
}