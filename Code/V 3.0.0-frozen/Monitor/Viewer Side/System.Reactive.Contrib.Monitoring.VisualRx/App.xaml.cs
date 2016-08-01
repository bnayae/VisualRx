#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.ServiceModel;
using System.Windows;
using System.Security;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region OnStartup

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Task<VisualRxInitResult> info = 
                VisualRxSettings.Initialize(
                    LocalProxy.Create());
            info.ContinueWith(inf => Trace.WriteLine(inf));

            UIPluginHelper.SetDispatcher();

            MarbleController.Init(Resources);
        }

        #endregion // OnStartup

        #region OnExit

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs"/> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            MarbleController.CloseHosting();
            base.OnExit(e);
        }

        #endregion // OnExit
    }
}