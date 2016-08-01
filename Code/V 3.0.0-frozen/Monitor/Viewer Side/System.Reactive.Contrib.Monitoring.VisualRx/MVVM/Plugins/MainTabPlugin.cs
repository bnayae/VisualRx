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

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Tab selector plug-in (for main (all categories) tab)
    /// </summary>
    public class MainTabPlugin : ITabPlugin
    {
        //private const string MAIN_DIAGRAM_KEY = "All";
        private ITabModel _tabModel = new MarbleDiagramsViewModel(
                    Constants.MAIN_DIAGRAM_KEY, null, TabKind.Marble);

        #region SelectTemplate

        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="tabModel">The tab model.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public DataTemplate SelectTemplate(ITabModel tabModel, FrameworkElement element)
        {
            return element.FindResource("DiagramsTemplate") as DataTemplate;
        }

        #endregion SelectTemplate

        #region TabModel

        /// <summary>
        /// Gets the tab model.
        /// </summary>
        public ITabModel TabModel
        {
            get
            {
                return _tabModel;
            }
        }

        #endregion TabModel

        #region Order

        /// <summary>
        /// Gets the tab's order.
        /// </summary>
        public double Order
        {
            get { return -1; }
        }

        #endregion Order
    }
}