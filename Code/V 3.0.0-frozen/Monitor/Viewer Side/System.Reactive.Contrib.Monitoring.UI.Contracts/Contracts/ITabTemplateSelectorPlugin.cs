#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Template selector plug-in contract that can be use to select
    /// an tab template
    /// </summary>
    public interface ITabPlugin
    {
        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="tabModel">The tab model.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        DataTemplate SelectTemplate(ITabModel tabModel, FrameworkElement element);

        /// <summary>
        /// Gets the tab model.
        /// </summary>
        ITabModel TabModel { get; }

        /// <summary>
        /// Gets the tab's order.
        /// </summary>
        double Order { get; }
    }
}