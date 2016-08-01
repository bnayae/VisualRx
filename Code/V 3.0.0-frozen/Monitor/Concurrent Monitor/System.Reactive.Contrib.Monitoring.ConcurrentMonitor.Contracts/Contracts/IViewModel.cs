#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Windows.Media;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    public interface IViewModel
    {
        IEnumerable<MarbleBase> FlatData { get; }
        IEnumerable<IVMDataStream> HierarchicRoot { get; }
        IEnumerable<string> AllKeywords { get; }
    }
}