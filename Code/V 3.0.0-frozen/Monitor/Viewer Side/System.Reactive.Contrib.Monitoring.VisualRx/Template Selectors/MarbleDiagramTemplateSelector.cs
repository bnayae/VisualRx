#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows;
using System.Windows.Controls;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Marble template selector
    /// </summary>
    public class MarbleDiagramTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// select the template for the marble diagram
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var marbleDiagram = item as IMarbleDiagramContext;

            IMarblePanelPlugin[] plugins =
                MarbleController.MarbleDiagramTemplateSelectorPlugin.ToArray();

            foreach (var plugin in plugins)
            {
                try
                {
                    DataTemplate template = plugin.SelectTemplate(marbleDiagram, element);
                    if (template != null)
                        return template;
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("Fail to select template for marble diagram, plugins: {0}, \r\n\terror = {1}",
                        plugin, ex);
                    return null;
                }

                #endregion Exception Handling
            }
            return element.FindResource("DiagramTemplate") as DataTemplate;
        }
    }
}