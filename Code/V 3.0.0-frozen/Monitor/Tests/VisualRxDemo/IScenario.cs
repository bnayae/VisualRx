using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo
{
    [InheritedExport]
    public interface IScenario
    {
        string Title { get; }
        string Sample { get; }
        double Order { get; }
        ICommand Invoke { get; }
    }
}
