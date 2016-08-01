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
    public class InvokeCommand : ICommand
    {
        public InvokeCommand(Action click)
        {
            Click = click;
        }

        public Action Click { get; private set; }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Click();
        }
    }

}
