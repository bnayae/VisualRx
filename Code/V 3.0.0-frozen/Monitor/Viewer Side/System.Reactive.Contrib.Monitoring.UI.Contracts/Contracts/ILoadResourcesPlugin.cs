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
    /// Resource loader plug-in contract that can be use to select
    /// an item template
    /// </summary>
    public interface ILoadResourcesPlugin
    {
        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns></returns>
        ResourceDictionary[] GetResources();
    }
}