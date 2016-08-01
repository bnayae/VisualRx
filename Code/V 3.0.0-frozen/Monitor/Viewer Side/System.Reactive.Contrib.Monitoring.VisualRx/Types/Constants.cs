#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    public abstract class Constants : System.Reactive.Contrib.Monitoring.Contracts.Constants
    {
        public const int ReplayTimeoutSec = 2;
        public const string MAIN_DIAGRAM_KEY = "All";
        //public const string GRID_DIAGRAM_KEY = "Grid";
    }
}