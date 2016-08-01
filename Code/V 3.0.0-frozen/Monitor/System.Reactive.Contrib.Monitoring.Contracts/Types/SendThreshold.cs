#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;

#endregion Using


namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// the threshold of the send buffering
    /// </summary>
    public class SendThreshold 
    {
        public SendThreshold(TimeSpan windowDuration, int windowCount)
        {
            WindowDuration = windowDuration;
            WindowCount = windowCount;
        }

        public TimeSpan WindowDuration { get; set; }
        public int WindowCount { get; set; }
    }
}