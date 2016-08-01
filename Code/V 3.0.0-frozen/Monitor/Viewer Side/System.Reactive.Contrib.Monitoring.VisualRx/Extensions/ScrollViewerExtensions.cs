#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Threading;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// controlling the scroll to end attached property
    /// </summary>
    public static class ScrollViewerExtensions
    {
        private const int SCROLL_GAP = 10;

        #region GetIsScrollToEnd

        /// <summary>
        /// Gets the is scroll to end.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static bool GetIsScrollToEnd(DependencyObject obj)
        {
            if (obj == null)
                return false;
            return (bool)obj.GetValue(IsScrollToEndProperty);
        }

        #endregion // GetIsScrollToEnd

        #region SetIsScrollToEnd

        /// <summary>
        /// Sets the is scroll to end.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsScrollToEnd(DependencyObject obj, bool value)
        {
            if (obj == null)
                return;
            obj.SetValue(IsScrollToEndProperty, value);
        }

        #endregion // SetIsScrollToEnd

        #region IsScrollToEndProperty

        /// <summary>
        /// IsScrollToEnd Property
        /// </summary>
        public static readonly DependencyProperty IsScrollToEndProperty =
            DependencyProperty.RegisterAttached("IsScrollToEnd", typeof(bool), typeof(ScrollViewerExtensions), new UIPropertyMetadata(false, OnIsScrollToEndChanged));

        #endregion // IsScrollToEndProperty

        #region OnIsScrollToEndChanged

        /// <summary>
        /// Called when [is scroll to end changed].
        /// attach / detach from the scroll-bar events
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scroller = d as ScrollViewer;
            if (scroller == null)
                return;

            if ((bool)e.NewValue)
            {
                scroller.ScrollChanged += OnScrollChanged;
                scroller.IsMouseCaptureWithinChanged += OnIsMouseCaptureWithinChanged;
            }
            else
            {
                scroller.ScrollChanged -= OnScrollChanged;
                scroller.IsMouseCaptureWithinChanged -= OnIsMouseCaptureWithinChanged;
            }
        }

        #endregion // OnIsScrollToEndChanged

        #region OnIsMouseCaptureWithinChanged

        /// <summary>
        /// Called when [is mouse capture within changed].
        /// release scroll to end on manual scroll
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        static void OnIsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // TODO: do it only for the horizontal scroll
            //if ((bool)e.NewValue)
            //{
            //    ScrollViewer scroller = sender as ScrollViewer;
            //    SetIsScrollToEnd(scroller, false);
            //}
        }

        #endregion // OnIsMouseCaptureWithinChanged

        #region OnScrollChanged

        /// <summary>
        /// Called when [scroll changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.ScrollChangedEventArgs"/> instance containing the event data.</param>
        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroller = sender as ScrollViewer;
            if (scroller == null || scroller.IsMouseCaptureWithin)
                return;

            var context = scroller.DataContext as MarbleDiagramsViewModel;
            double lastScroll = context == null ? 0 : context.LastScroll;
            double position = scroller.ExtentWidth - scroller.ViewportWidth;
            if (lastScroll + SCROLL_GAP < position || lastScroll > position)
            {
                if (Monitor.TryEnter(context))
                {
                    try
                    {
                        scroller.ScrollToHorizontalOffset(position);
                        if (context != null)
                            context.LastScroll = position;
                    }
                    finally
                    {
                        Monitor.Exit(context);
                    }
                }
            }
        }

        #endregion // OnScrollChanged
    }
}