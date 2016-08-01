#region Using

using System.Diagnostics;
using System.Threading;


#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts.Internals
{
    /// <summary>
    /// Trace source helper
    /// </summary>
    public static class TraceSourceMonitorHelper
    {
        private const string TRACE_NAME = "VisualRx.Log";

        private static int _id = 0;
        private static readonly TraceSource _trace = new TraceSource(TRACE_NAME);

        #region Debug

        /// <summary>
        /// Trace debug (Verbose) level.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void Debug(string format, params object[] args)
        {
            int id = Interlocked.Increment(ref _id);
            string text = string.Format(format, args);
            _trace.TraceData(TraceEventType.Verbose, id, text + Environment.NewLine);
        }

        #endregion Debug

        #region Info

        /// <summary>
        /// Trace information.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void Info(string format, params object[] args)
        {
            int id = Interlocked.Increment(ref _id);
            string text = string.Format(format, args);
            _trace.TraceData(TraceEventType.Information, id, text + Environment.NewLine);
        }

        #endregion Info

        #region Warn

        /// <summary>
        /// Trace warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void Warn(string format, params object[] args)
        {
            int id = Interlocked.Increment(ref _id);
            string text = string.Format(format, args);
            _trace.TraceData(TraceEventType.Warning, id, text + Environment.NewLine);
        }

        #endregion Warn

        #region Error

        /// <summary>
        /// Trace error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void Error(string format, params object[] args)
        {
            int id = Interlocked.Increment(ref _id);
            string text = string.Format(format, args);
            _trace.TraceData(TraceEventType.Error, id, text + Environment.NewLine);
        }

        #endregion Error

        #region // AddListener

        ///// <summary>
        ///// Adds the listener.
        ///// </summary>
        ///// <param name="listeners">The listeners.</param>
        //public static void AddListener(params TraceListener[] listeners)
        //{
        //    _trace.Listeners.AddRange(listeners);
        //}

        #endregion // AddListener
    }
}