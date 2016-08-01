#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.SamplePlugin
{
    public class ImageMapperPlugin : ILineHeaderImagePlugin
    {
        private static readonly string ASSEMBLY_NAME = UIPluginHelper.GetAssemblyName();
        private readonly Uri STRESS_IMG = UIPluginHelper.GetPackResourceUri(ASSEMBLY_NAME, "Images/clicknrun.png");
        private readonly Uri SEC_IMG = UIPluginHelper.GetPackResourceUri(ASSEMBLY_NAME, "Images/Clock.png");

        public ImageSource Convert(string text)
        {
            text = text.ToLower();
            ImageSource img = null;
            if (text.StartsWith("stress", StringComparison.InvariantCultureIgnoreCase))
                img = new BitmapImage(STRESS_IMG);
            else if (text.StartsWith("sec", StringComparison.InvariantCultureIgnoreCase))
                img = new BitmapImage(SEC_IMG);
            return img;
        }
    }
}