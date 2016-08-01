#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// general command
    /// </summary>
    public class Command : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        public Command(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            if (_canExecute == null)
                _canExecute = param => true;
        }

        #endregion Ctor

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged = (s, e) => { };

        #region PublishCanExecuteChanged

        /// <summary>
        /// Publishes the can execute changed.
        /// </summary>
        public void PublishCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion PublishCanExecuteChanged

        #region CanExecute

        /// <summary>
        /// Defines the method that determines whether the Command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the Command.  If the Command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this Command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        #endregion CanExecute

        #region Execute

        /// <summary>
        /// Defines the method to be called when the Command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the Command.  If the Command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion Execute
    }
}