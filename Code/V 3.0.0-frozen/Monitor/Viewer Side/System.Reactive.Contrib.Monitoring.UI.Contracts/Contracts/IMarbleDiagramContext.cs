#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    ///  used for binding from the marble base to the parent
    /// </summary>
    public interface IMarbleDiagramContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        int Height { get; set; }

        /// <summary>
        /// Gets the positioning strategy.
        /// </summary>
        MarblePositioningStrategy PositioningStrategy { get; }

        /// <summary>
        /// Gets the local time in seconds.
        /// </summary>
        /// <param name="itemTotalSecond">The item total second.</param>
        /// <returns></returns>
        double GetLocalTimeInSeconds(double itemTotalSecond);

        /// <summary>
        /// Gets the unit.
        /// </summary>
        MarbleUnit Unit { get; }

        /// <summary>
        /// Gets the main context.
        /// </summary>
        IMainContext MainContext { get; }

        /// <summary>
        /// Gets the color.
        /// </summary>
        SolidColorBrush Color { get; }
    }
}