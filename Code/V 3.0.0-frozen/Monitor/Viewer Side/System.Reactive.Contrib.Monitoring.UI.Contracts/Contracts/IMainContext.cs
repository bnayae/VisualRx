#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    ///  the main context
    /// </summary>
    public interface IMainContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the width of the diagram.
        /// </summary>
        /// <value>
        /// The width of the diagram.
        /// </value>
        double DiagramWidth { get; }

        /// <summary>
        /// Translates the offset.
        /// </summary>
        /// <param name="itemWrapper">The item wrapper.</param>
        /// <returns></returns>
        double TranslateOffset(MarbleItemViewModel itemWrapper);

        /// <summary>
        /// Gets or sets the diagram scale.
        /// </summary>
        /// <value>
        /// The diagram scale.
        /// </value>
        double DiagramScale { get; set; }

        /// <summary>
        /// Gets the keyword.
        /// </summary>
        string Keyword { get; }

        /// <summary>
        /// Gets or sets the canvas left.
        /// </summary>
        /// <value>
        /// The canvas left.
        /// </value>
        double CanvasLeft { get; set; }
    }
}