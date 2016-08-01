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
    /// Tab selector plugin (for main (all categories) tab)
    /// </summary>
    public class GridTabPlugin : ITabPlugin
    {
        private const string GRID_DIAGRAM_KEY = "Grid";

        private ITabModel _tabModel = new MarbleDiagramsViewModel(
                    GRID_DIAGRAM_KEY, null, TabKind.Flat);

        #region SelectTemplate

        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="tabModel">The tab model.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public DataTemplate SelectTemplate(ITabModel tabModel, FrameworkElement element)
        {
            var dataTemplate = element.FindResource("DiagramsGridTemplate") as DataTemplate;
            return dataTemplate;
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
            get { return -10; }
        }

        #endregion Order
    }
}