#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Scroll-bar scrolling kind
    /// </summary>
    public enum ScrollKind
    {
        /// <summary>
        /// manual scrolling
        /// </summary>
        ManualScroll,

        /// <summary>
        /// active scroll to end
        /// </summary>
        ScrollToEnd,
    }
}