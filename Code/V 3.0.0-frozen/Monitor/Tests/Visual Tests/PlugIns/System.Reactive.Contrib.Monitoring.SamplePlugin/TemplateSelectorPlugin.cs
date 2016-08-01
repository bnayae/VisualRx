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

namespace System.Reactive.Contrib.Monitoring.SamplePlugin
{
    /// <summary>
    /// Plug-in that can set a special OnNext template
    /// </summary>
    public class PseudoTemplateSelectorPlugin : IMarbleItemPlugin
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
            bool startWith = item.Name.ToLower().StartsWith("sec");
            bool isDivBy10 = Convert.ToInt32(item.RawValue) % 10 == 0;
            if (startWith && isDivBy10)
            {
                template = element.FindResource("CustomMarbleNextTemplate") as DataTemplate;
            }
            return template;
        }

        #endregion SelectTemplate
    }
}