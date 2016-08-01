#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Views;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = MarbleController.ViewModel;
        }

        #endregion // Ctor

        #region Event Handlers

        #region OnHideHandler

        /// <summary>
        /// Called when [hide handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnHideHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = Windows.WindowState.Minimized;
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnHideHandler

        #region OnCloseHandler

        /// <summary>
        /// Called when [close handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCloseHandler(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
            this.Close();
        }

        #endregion // OnCloseHandler

        #region OnDragHandler

        /// <summary>
        /// Called when [drag handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnDragHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Maximized)
                {
                    this.Top = 1;
                    WindowState = WindowState.Normal;
                }
                this.DragMove();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnDragHandler

        #region OnPauseHandler

        /// <summary>
        /// Called when [pause handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnPauseHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                MarbleController.ViewModel.TogglePause();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnPauseHandler

        #region OnMinimizeHandler

        /// <summary>
        /// Called when [minimize handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnMinimizeHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowState = WindowState.Minimized;
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnMinimizeHandler

        #region OnResize

        /// <summary>
        /// Called when [resize].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void OnResize(object sender, DragDeltaEventArgs e)
        {
            try
            {
                var str = (string)((Thumb)sender).Tag;

                if (str.Contains("T"))
                {
                    double height = Math.Min(Math.Max(this.MinHeight, this.ActualHeight - e.VerticalChange), this.MaxHeight);
                    double gap = (height - this.Height);
                    this.Height += gap;
                    this.Top -= gap;
                }
                if (str.Contains("L"))
                {
                    double width = Math.Min(Math.Max(this.MinWidth, this.ActualWidth - e.HorizontalChange), this.MaxWidth);
                    double gap = (width - this.Width);

                    this.Width += gap;
                    this.Left -= gap;
                }
                if (str.Contains("B"))
                {
                    this.Height = Math.Min(Math.Max(this.MinHeight, this.ActualHeight + e.VerticalChange), this.MaxHeight);
                }
                if (str.Contains("R"))
                {
                    this.Width = Math.Min(Math.Max(this.MinWidth, this.ActualWidth + e.HorizontalChange), this.MaxWidth);
                }

                e.Handled = true;

            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnResize

        #region OnAboutHandler

        /// <summary>
        /// Called when [about handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAboutHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                HelpWindow win = null;
                try
                {
                    win = new HelpWindow();
                    win.ShowDialog();
                }
                finally
                {
                    win.Close();
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnAboutHandler

        #region OnSettingHandler

        /// <summary>
        /// Called when [setting handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSettingHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingWindow win = null;
                try
                {
                    win = new SettingWindow();
                    win.Owner = this;
                    win.ShowDialog();
                }
                finally
                {
                    win.Close();
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnSettingHandler

        #region OnShowSampleHandler

        /// <summary>
        /// Called when [show sample handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnShowSampleHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MarbleController.ViewModel.SamplesWidth < 50)
                    MarbleController.ViewModel.SamplesWidth = MarbleDiagramsTabsViewModel.DEFAULT_SAMPLE_WIDTH;
                else
                {
                    MarbleController.ViewModel.SamplesWidth = 0;
                    MarbleController.ViewModel.CurrentSample = null;
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("UI event handler failed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // OnShowSampleHandler

        #endregion // Event Handlers
    }
}