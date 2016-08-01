#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// the marble diagram positioning strategy
    /// </summary>
    public enum MarblePositioningStrategy
    {
        /// <summary>
        /// Global time will show the marble on a timeline with
        /// a correlation between all marble diagrams
        /// within the current tab
        /// </summary>
        GlobalTime,

        /// <summary>
        /// Private time will conduct the diagram timeline on a single diagram base
        /// </summary>
        PrivateTime,

        /// <summary>
        /// Sequence will present the marbles in correlation
        /// to their creation order regardless of the creation time
        /// </summary>
        Sequence
    }
}