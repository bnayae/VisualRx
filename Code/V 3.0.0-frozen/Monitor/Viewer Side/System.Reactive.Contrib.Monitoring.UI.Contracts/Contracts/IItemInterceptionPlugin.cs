#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Plug-ins that intercept the data flow
    /// </summary>
    public interface IItemInterceptionPlugin
    {
        /// <summary>
        /// Appends the marble.
        /// </summary>
        /// <param name="value">The value.</param>
        void AppendMarble(MarbleBase value);
    }
}