#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.SamplePlugin
{
    /// <summary>
    /// load resources plug-in
    /// </summary>
    public class LoadResourcesPlugin : ILoadResourcesPlugin
    {
        private readonly string ASSEMBLY_NAME = UIPluginHelper.GetAssemblyName();

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns></returns>
        public ResourceDictionary[] GetResources()
        {
            Uri uri = UIPluginHelper.GetResourceUri(ASSEMBLY_NAME, "CustomMarble.xaml");
            ResourceDictionary myResource = new ResourceDictionary();
            myResource.Source = uri;
            return new ResourceDictionary[] { myResource };
        }
    }
}