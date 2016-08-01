#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
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
    public class MarbleTemplateSelector : DataTemplateSelector
    {
        #region SelectTemplate

        /// <summary>
        /// select the template for the marble item
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var marble = item as MarbleItemViewModel;

            switch (marble.Item.Kind)
            {
                case System.Reactive.Contrib.Monitoring.Contracts.MarbleKind.OnNext:
                    {
                        DataTemplate template =
                            (from plugin in MarbleController.MarbleTemplateSelectorPlugin
                             let t = TrySelectCustomTemplate(plugin, marble.Item, element)
                             where t != null
                             select t).FirstOrDefault();

                        if (template != null)
                            return template;

                        return element.FindResource("MarbleNextTemplate") as DataTemplate;
                    }
                case System.Reactive.Contrib.Monitoring.Contracts.MarbleKind.OnError:
                    return element.FindResource("MarbleErrorTemplate") as DataTemplate;
                case System.Reactive.Contrib.Monitoring.Contracts.MarbleKind.OnCompleted:
                    return element.FindResource("MarbleCompleteTemplate") as DataTemplate;
            }
            return base.SelectTemplate(item, container);
        }

        #endregion SelectTemplate

        #region TrySelectCustomTemplate

        /// <summary>
        /// Tries the select custom template.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="element">The element.</param>
        /// <param name="plugin">The plugin.</param>
        /// <returns></returns>
        private DataTemplate TrySelectCustomTemplate(
            IMarbleItemPlugin plugin,
            MarbleBase item,
            FrameworkElement element)
        {
            try
            {
                return plugin.SelectTemplate(item, element);
            }

            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to select template for marble item, plugins: {0}, item = {1}, \r\n\terror = {2}",
                    plugin, item.Value, ex);
                return null;
            }

            #endregion Exception Handling
        }

        #endregion TrySelectCustomTemplate
    }
}