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
    /// an marble diagram template
    /// use this option in order to replace or manipulate the the marble-diagram template. 
    /// </summary>
    public interface IMarblePanelPlugin
    {
        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="marbleDiagram">The marble diagram.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        DataTemplate SelectTemplate(
            IMarbleDiagramContext marbleDiagram,
            FrameworkElement element);
    }
}