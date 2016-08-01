#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows;

#endregion Using

namespace Bnaya.Samples
{
    /// <summary>
    /// Plug-in that can set a special OnNext template
    /// </summary>
    public class MarbleDiagramTemplateSelectorPlugin : IMarblePanelPlugin
    {
        #region SelectTemplate

        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="item">The diagram.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public DataTemplate SelectTemplate(IMarbleDiagramContext marbleDiagram, FrameworkElement element)
        {
            DataTemplate template = null;
            if (marbleDiagram.Name == "Stock")
            {
                marbleDiagram.Height = 70;
                //template = element.FindResource("StockDiagramTemplate") as DataTemplate;
            }
            return template;
        }

        #endregion SelectTemplate
    }
}