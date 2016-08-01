#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// View model contract
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        /// <value>
        /// The width of the diagram.
        /// </value>
        int Width { get; }
    }
}