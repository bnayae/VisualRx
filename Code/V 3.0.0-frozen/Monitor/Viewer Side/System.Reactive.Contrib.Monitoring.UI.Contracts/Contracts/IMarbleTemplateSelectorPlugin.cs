#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Windows;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Template selector plug-in contract that can be use to select
    /// an item template
    /// </summary>
    public interface IMarbleItemPlugin
    {
        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        DataTemplate SelectTemplate(MarbleBase item, FrameworkElement element);
    }
}