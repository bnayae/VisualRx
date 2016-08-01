#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// present a data stream
    /// </summary>
    public interface IVMDataStream
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        string FullName { get; }
        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        string[] Keywords { get; }
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        IList<IVMDataStream> Children { get; }
        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        ListCollectionView Items { get; }
    }
}