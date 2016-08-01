#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Security;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Helper
    /// </summary>
    public static class UIPluginHelper
    {
        #region GetAssemblyName

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyName()
        {
            return Assembly.GetCallingAssembly().GetName().Name;
        }

        #endregion GetAssemblyName

        #region GetResourceUri

        /// <summary>
        /// Gets the relative resource URI.
        /// </summary>
        /// <param name="assemblyName">the assembly name</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static Uri GetResourceUri(string assemblyName, string relativePath)
        {
            string format = @"/{0};component/Resources/{1}";
            var uri = new Uri(string.Format(format, assemblyName, relativePath), UriKind.Relative);
            return uri;
        }

        #endregion GetResourceUri

        #region GetPackResourceUri

        /// <summary>
        /// Gets the relative resource URI.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static Uri GetPackResourceUri(string assemblyName, string relativePath)
        {
            string format = @"pack://application:,,,/{0};component/{1}";
            string path = string.Format(format, assemblyName, relativePath);
            var uri = new Uri(path);
            return uri;
        }

        /// <summary>
        /// Gets the relative resource URI.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static Uri GetPackResourceUri(string relativePath)
        {
            string asm = Assembly.GetCallingAssembly().GetName().Name;
            return GetPackResourceUri(asm, relativePath);

        //    string format = @"pack://application:,,,;component/{0}";
        //    string path = string.Format(format, relativePath);
        //    var uri = new Uri(path);
        //    return uri;
        }

        #endregion GetPackResourceUri

        #region CurrentDispatcher

        /// <summary>
        /// Gets the current dispatcher.
        /// </summary>
        public static Dispatcher CurrentDispatcher { get; private set; }

        #endregion // CurrentDispatcher

        #region SetDispatcher

        /// <summary>
        /// Sets the dispatcher.
        /// </summary>
        [SecuritySafeCritical]
        public static void SetDispatcher()
        {
            if (CurrentDispatcher != null)
                throw new InvalidOperationException("Only single assignment is allowed");
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
            CurrentDispatcher.Hooks.OperationAborted += (s,e) =>
                TraceSourceMonitorHelper.Warn("UI abort: {0}", e.Operation.Status);
            CurrentDispatcher.UnhandledException += (s,e) =>
                TraceSourceMonitorHelper.Error("UI unhandled error: {0}", e.Exception);
        }

        #endregion // SetDispatcher
    }
}