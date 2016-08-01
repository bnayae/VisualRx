#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows;

#endregion Using

namespace Bnaya.Samples
{
    /// <summary>
    /// Plug-in that can set a special OnNext template
    /// </summary>
    public class MarbleTemplateSelectorPlugin : IMarbleItemPlugin
    {
        #region SelectTemplate

        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public DataTemplate SelectTemplate(MarbleBase item, FrameworkElement element)
        {
            DataTemplate template = null;
            if (item.Name == "Stock")
            {
                IList<double> lst = item.RawValue as IList<double>;
                if (lst != null)
                {
                    if (lst[0] < lst[1])
                        template = element.FindResource("StockUpTemplate") as DataTemplate;
                    else
                        template = element.FindResource("StockDownTemplate") as DataTemplate;
                }
                else
                {
                    Trace.WriteLine(item.RawValue.GetType().Name);
                }
            }
            return template;
        }

        #endregion SelectTemplate
    }
}