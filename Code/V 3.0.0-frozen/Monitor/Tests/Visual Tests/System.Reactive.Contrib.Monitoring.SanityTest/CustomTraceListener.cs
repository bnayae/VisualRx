#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

#endregion Using

namespace System.Reactive.Contrib.TestMonitor
{
    public class CustomTraceListener : TraceListener
    {
        private static ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        public override void Write(string message)
        {
            _queue.Enqueue(message);
        }

        public override void WriteLine(string message)
        {
            _queue.Enqueue(message);
        }
    }
}