#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Indicate whether the data has to be flatten
    /// </summary>
    public enum TabKind
    {
        /// <summary>
        /// Marble hierarchic view
        /// </summary>
        Marble,

        /// <summary>
        /// flat view
        /// </summary>
        Flat
    }
}