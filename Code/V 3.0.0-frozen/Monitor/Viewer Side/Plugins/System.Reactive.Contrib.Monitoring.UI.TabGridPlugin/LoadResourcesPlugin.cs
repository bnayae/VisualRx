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
    public class LoadResourcesPlugin : ILoadResourcesPlugin
    {
        private readonly string ASSEMBLY_NAME = UIPluginHelper.GetAssemblyName();

        /// <summary>
        /// load resources plug-in
        /// </summary>
        /// <returns></returns>
        public ResourceDictionary[] GetResources()
        {
            //Uri uri = new Uri("/" +
            //    System.Reflection.Assembly.GetExecutingAssembly().GetName().Name +
            //    ";component/Resources/", UriKind.Relative);
            var uri = UIPluginHelper.GetResourceUri(ASSEMBLY_NAME, "CustomResource.xaml");
            ResourceDictionary myResource = new ResourceDictionary();
            myResource.Source = uri;
            return new ResourceDictionary[] { myResource };
        }
    }
}