#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Contrib.Monitoring.UI.Properties;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using System.ServiceModel;
using System.Security;
using System.Messaging;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// MVC controller hold the view model and
    /// get invoked from the service
    /// </summary>
    /// <example>
    /// Command-line Sample: PluginFolder:"C:\tmp" PluginFolder:"Plug-ins\Data"
    /// </example>
    internal static class MarbleController
    {
        // Regex = start with PluginFolder: group from " to "
        private const string DEF_PLUGIN_FOLDER = "Plug-ins";
        private const string PLUGIN_FOLDER_REGEX = "PluginFolder:\"(?<Path>[^\"]+)\"";

        private static readonly Regex _regex = new Regex(PLUGIN_FOLDER_REGEX);
        private static ServiceHost _host;
        //private static DispatcherScheduler _uiScheduler;
        private static ViewModel _vm;

        #region Init

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public static void Init(ResourceDictionary dictionary)
        {
            //_uiScheduler = DispatcherScheduler.Current;
            _vm = new ViewModel();

            //FixConfiguration();
            //PluginDiscovery(ConfigurationController.Value);
            //RefreshPlugins();

            //ViewModel = new MarbleDiagramsTabsViewModel();

            //MergeResuorces(dictionary);

            _host = new ServiceHost(typeof(MonitorServiceAdapter));
            try
            {
                string queueName = (from endpoint in (_host).Description.Endpoints
                                    where endpoint.Binding is NetMsmqBinding
                                    from segment in endpoint.Address.Uri.Segments
                                    select segment).LastOrDefault();
                CreateQueueIfNotExists(queueName);
                _host.Open();
            }
            #region Exception Handling

            catch (TypeInitializationException ex)
            {
                var innerEx = ex.InnerException as DllNotFoundException;
                if (innerEx != null)
                {
                    string txt = string.Format(@"Check if all endpoint are supported 
(for example: net.msmq supported only when MSMQ install on the machine)
or remove this endpoint from the application configuration.

{0}", innerEx.Message);
                    MessageBox.Show(txt);
                }
                else
                    MessageBox.Show(ex.ToString());
            }

            #endregion Exception Handling
        }

        #endregion // Init

        #region AddMarbleItem

        /// <summary>
        /// Add item into the item flow
        /// </summary>
        /// <param name="items"></param>
        public static void AddMarbleItem(MarbleBase[] items)
        {
            _vm.AddRange(items);
        }

        #endregion AddMarbleItem

        #region EnsureDirectoryExists

        /// <summary>
        /// Ensures the directory exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static bool EnsureDirectoryExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(DEF_PLUGIN_FOLDER);
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to create plug-in polder: {0}\r\n{1}",
                    DEF_PLUGIN_FOLDER, ex);
                return false;
            }

            #endregion Exception Handling
            return true;
        }

        #endregion // EnsureDirectoryExists

        #region GetPluginFoldersFromCommandline

        /// <summary>
        /// Gets the plug-in folders from Command-line.
        /// </summary>
        /// <returns></returns>
        private static string[] GetPluginFoldersFromCommandline()
        {
            var result = from m in _regex.Matches(Environment.CommandLine).Cast<Match>()
                         select m.Groups["Path"].Value;
            return result.ToArray();
        }

        #endregion GetPluginFoldersFromCommandline

        #region CreateQueueIfNotExists

        /// <summary>
        /// Creates the queue if not exists.
        /// </summary>
        /// <param name="queueName"></param>
        [SecuritySafeCritical]
        private static void CreateQueueIfNotExists(string queueName)
        {
            #region Validation

            if (string.IsNullOrWhiteSpace(queueName))
                return;

            #endregion // Validation

            try
            {
                string queueFullName = ".\\private$\\" + queueName;
                if (!MessageQueue.Exists(queueFullName))
                    MessageQueue.Create(queueFullName, false);
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to create MSMQ queue: {0}", ex);
            }

            #endregion Exception Handling
        }

        #endregion // CreateQueueIfNotExists
        
		#region CloseHosting

        /// <summary>
        /// Closes the hosting.
        /// </summary>
        public static void CloseHosting()
        {
            try
            {
                if (_host != null)
                    _host.Close();
                _host = null;
            }
            #region Exception Handling

            catch
            {
                // do nothing
            }

            #endregion // Exception Handling
        }

		#endregion // CloseHosting
    }
}