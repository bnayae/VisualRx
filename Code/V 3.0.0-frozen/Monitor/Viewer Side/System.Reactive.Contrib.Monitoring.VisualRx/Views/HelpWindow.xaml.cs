#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Input;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region OnClose

        /// <summary>
        /// Called when [close].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion // OnClose

        #region OnDragHandler

        /// <summary>
        /// Called when [drag handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnDragHandler(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                this.Top = 1;
                WindowState = WindowState.Normal;
            }
            this.DragMove();
        }

        #endregion // OnDragHandler

        #region Version

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); } }

        #endregion // Version

        #region HandleRequestNavigate

        /// <summary>
        /// Handles the request navigate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.RequestNavigateEventArgs"/> instance containing the event data.</param>
        [SecuritySafeCritical]
        private void HandleRequestNavigate(object sender, Windows.Navigation.RequestNavigateEventArgs e)
        {
            var url = e.Uri.ToString();
            System.Diagnostics.Process.Start(url);
        }

        #endregion // HandleRequestNavigate
    }
}