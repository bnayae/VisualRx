#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// marble diagram duration unit
    /// </summary>
    public enum MarbleUnit
    {
        /// <summary>
        /// Seconds 0.001
        /// </summary>
        Seconds0001,

        /// <summary>
        /// Seconds 0.01
        /// </summary>
        Seconds001,

        /// <summary>
        /// Seconds 0.1
        /// </summary>
        Seconds01,

        /// <summary>
        /// Seconds
        /// </summary>
        Seconds,

        /// <summary>
        /// Minutes
        /// </summary>
        Minute,

        /// <summary>
        /// Hour
        /// </summary>
        Hour,

        /// <summary>
        /// Day
        /// </summary>
        Day,
    }
}