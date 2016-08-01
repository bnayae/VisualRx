#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Security;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = ConfigurationVM.Instance;
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

        #region OnCancel

        /// <summary>
        /// Called when [cancelled].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            ConfigurationController.Load();
            Close();
        }

        #endregion // OnCancel

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

        #region OnSave

        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSave(object sender, RoutedEventArgs e)
        {
            Exception ex = ConfigurationController.Save();
            if (ex == null)
                Close();
            else
                MessageBox.Show(ex.ToString(), "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion // OnSave

        #region OnSaveAndRestart

        /// <summary>
        /// Called when [save and restart].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        [SecuritySafeCritical]
        private void OnSaveAndRestart(object sender, RoutedEventArgs e)
        {
            Exception ex = ConfigurationController.Save();
            if (ex == null)
            {
                Close();
                MarbleController.CloseHosting();

                var commandArgs = Environment.GetCommandLineArgs();
                string path = commandArgs[0];
                string args = string.Join(" ", commandArgs.Skip(1));
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args,
                    UseShellExecute = true,
                };
                Process p = Process.Start(startInfo);
                p.WaitForInputIdle();
                Application.Current.Shutdown();
            }
            else
                MessageBox.Show(ex.ToString(), "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion // OnSaveAndRestart

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
