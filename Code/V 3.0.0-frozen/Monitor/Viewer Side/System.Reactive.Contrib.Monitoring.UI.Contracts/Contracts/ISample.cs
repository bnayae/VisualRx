using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    [InheritedExport]
    public interface ISample: ICommand
    {
        string Title { get; }
        string Query { get; }
        double Order { get; }

        string CommandText { get; }
    }
}
