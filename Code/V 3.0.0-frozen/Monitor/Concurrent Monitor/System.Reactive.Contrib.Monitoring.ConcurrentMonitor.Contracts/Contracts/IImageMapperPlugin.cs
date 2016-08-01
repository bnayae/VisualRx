#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Mapping image plug-in (convert text to ImageSource)
    /// used to replace the diagram's image.
    /// </summary>
    public interface ILineHeaderImagePlugin
    {
        /// <summary>
        /// Converts the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        ImageSource Convert(string text);
    }
}