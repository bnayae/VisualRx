#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Input;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Base class for plug-in 
    /// </summary>
    public abstract class SampleBase<T> : ISample, INotifyPropertyChanged
   {
        private IDisposable _subscriptionToken;

        #region ICommand Members

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        bool ICommand.CanExecute(object parameter) => true;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        async void ICommand.Execute(object parameter)
        {
            await Task.Delay(1).ConfigureAwait(false);
            if (_subscriptionToken == null)
            {
                _subscriptionToken = OnQuery()
                    .Timeout(TimeSpan.FromSeconds(20))
                    .DefaultIfEmpty()
                    .Subscribe(m => { }, ex => { }, () => 
                        {
                            try
                            {
                                _subscriptionToken?.Dispose();
                            }
                            catch { }
                            _subscriptionToken = null;
                        });
                CommandText = "Stop";
            }
            else
            {
                _subscriptionToken.Dispose();
                _subscriptionToken = null;
                CommandText = "Start";
            }
        }

        #endregion // ICommand Members

        #region Title

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public abstract string Title { get; }

        #endregion // Title

        #region Order

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public virtual double Order { get; } = 1000;

        #endregion // Order

        #region Query

        /// <summary>
        /// Called for the [query].
        /// </summary>
        /// <returns></returns>
        protected abstract IObservable<T> OnQuery();
        
        /// <summary>
        /// Gets the query.
        /// </summary>
        public abstract string Query { get; }

        #endregion // Query

        #region ToString

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Title;
        }

        #endregion // ToString

        #region CommandText

        private string _commandText = "Start";

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>
        /// The command text.
        /// </value>
        public string CommandText
        {
            get { return _commandText; }
            set
            {
                _commandText = value;
                OnPropertyChanged();
            }
        }

        #endregion // CommandText

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged


    }
}