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
    public class TabTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Selects a template from the Tab.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object model, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            var tabModel = model as ITabModel;

            ITabPlugin plugin =
                MarbleController.TabTemplateSelectorPlugins.FirstOrDefault(
                    p => p.TabModel.Keyword == tabModel.Keyword);

            if (plugin != null)
            {
                try
                {
                    DataTemplate template = plugin.SelectTemplate(tabModel, element);
                    if (template != null)
                        return template;
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("Fail to select template for tab, plugins: {0}, \r\n\terror = {1}",
                        plugin.TabModel.Keyword, ex);
                    return null;
                }

                #endregion Exception Handling
            }
            return element.FindResource("DiagramsTemplate") as DataTemplate;
        }
    }
}