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
        private const string DEF_PLUGIN_FOLDER = "Plugins";
        private const string PLUGIN_FOLDER_REGEX = "PluginFolder:\"(?<Path>[^\"]+)\"";

        private static readonly Regex _regex = new Regex(PLUGIN_FOLDER_REGEX);
        private static readonly ISubject<MarbleBase[]> _subject;
        private static ServiceHost _host;

        #region Constructors

        static MarbleController()
        {
            _subject = new Subject<MarbleBase[]>();
            _subject
                //.ObserveOn(DispatcherScheduler.Current)
                .Subscribe(OnNewItem);

        }

        #endregion Constructors

        #region PluginDiscovery

        /// <summary>
        /// Plug-ins the discovery.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        private static void PluginDiscovery(Configuration configuration)
        {
            try
            {
                #region AggregateCatalog aggregateCatalog = ...

                var allTabCatalog = new TypeCatalog(typeof(DefaultPlugin));
                var aggregateCatalog = new AggregateCatalog(allTabCatalog);

                #region Plug-in catalogs

                string[] commandlines = GetPluginFoldersFromCommandline();
                var paths = from path in commandlines
                                        .Union(configuration.PluginDiscoveryPaths.Select(v => v.Path))
                                        .Distinct()
                            where !string.IsNullOrWhiteSpace(path)
                            select path;

                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        var catalog = new DirectoryCatalog(path);
                        aggregateCatalog.Catalogs.Add(catalog);
                    }
                    else
                        TraceSourceMonitorHelper.Warn("Discovery path not found: {0}", path);
                }

                #endregion // Plug-in catalogs

                #endregion AggregateCatalog aggregateCatalog = ...

                var container = new CompositionContainer(aggregateCatalog);

                var plugins = container.GetExportedValues<IPluginBundle>().Distinct().ToArray();
                var plugActivation = configuration.PluginsActivation;
                if (plugActivation == null)
                {
                    configuration.PluginsActivation = new ConcurrentDictionary<Guid, bool>();
                    plugActivation = configuration.PluginsActivation;
                    foreach (var p in plugins)
                    {
                        plugActivation.TryAdd(p.Id, true);
                    }
                }
                Plugins = (from p in plugins
                           select new PluginVM(p)).ToArray();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to load plugins: {0}", ex);
            }

            #endregion Exception Handling
        }

        #endregion // PluginDiscovery

        #region FixConfiguration

        /// <summary>
        /// Fix the configuration.
        /// </summary>
        /// <returns></returns>
        private static void FixConfiguration()
        {
            var configuration = ConfigurationController.Value;

            for (int i = configuration.PluginDiscoveryPaths.Count - 1; i >= 0; i--)
            {
                string path = configuration.PluginDiscoveryPaths[i].Path;
                bool exist = EnsureDirectoryExists(path);
                if (!exist)
                    configuration.PluginDiscoveryPaths.RemoveAt(i);
            }
        }

        #endregion // FixConfiguration

        #region RefreshPlugins

        /// <summary>
        /// Refreshes the plug-ins.
        /// </summary>
        /// <param name="container">The container.</param>
        private static void RefreshPlugins()
        {
            var actives = (from p in Plugins
                          where p.Enabled
                          select p.Plugin).ToArray();

            LoadResourcesPlugins = (from p in actives
                                            let item = p.LoadResourcesPlugin
                                            where item != null
                                            select item).Distinct().ToArray();

            TabTemplateSelectorPlugins = (from p in actives
                                    let item = p.TabPlugin
                                    where item != null
                                    orderby item.Order
                                    select item).Distinct().ToArray();

            DiagramImageMappersPlugins = (from p in actives
                                            let item = p.LineHeaderImagePlugin
                                            where item != null
                                            select item).Distinct().ToArray();

            MarbleDiagramTemplateSelectorPlugin = (from p in actives
                                            let item = p.MarblePanelPlugin
                                            where item != null
                                            select item).Distinct().ToArray();

            MarbleTemplateSelectorPlugin = (from p in actives
                                            let item = p.MarbleItemPlugin
                                            where item != null
                                            select item).Distinct().ToArray();

            InterceptionPlugins = (from p in actives
                                            let item = p.InterceptionPlugin
                                            where item != null
                                            select item).Distinct().ToArray();
        }

        #endregion // RefreshPlugins

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

        #region Init

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public static void Init(ResourceDictionary dictionary)
        {
            FixConfiguration();
            PluginDiscovery(ConfigurationController.Value);
            RefreshPlugins();

            ViewModel = new MarbleDiagramsTabsViewModel();

            MergeResources(dictionary);

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

        #region MergedResources

        /// <summary>
        /// Merges the plug-in's resources.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        private static void MergeResources(ResourceDictionary dictionary)
        {
            try
            {
                foreach (var resourcePlugin in LoadResourcesPlugins)
                {
                    try
                    {
                        foreach (var resource in resourcePlugin.GetResources())
                        {
                            dictionary.MergedDictionaries.Add(resource);
                        }
                    }

                    #region Exception Handling

                    catch (Exception ex)
                    {
                        TraceSourceMonitorHelper.Error("Fail to get resource [plug-in = {0}]: {1}", resourcePlugin.GetType().Name, ex);
                    }

                    #endregion Exception Handling
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Fail to add plug-ins resources: {0}", ex);
            }

            #endregion Exception Handling
        }

        #endregion // MergedResources

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

        #region Plugins

        /// <summary>
        /// Gets or sets the plug-ins.
        /// </summary>
        /// <value>
        /// The plug-ins.
        /// </value>
        public static PluginVM[] Plugins { get; set; }

        #endregion // Plugins

        #region InterceptionPlugins

        /// <summary>
        /// Gets the interception plug-ins.
        /// </summary>
        public static IItemInterceptionPlugin[] InterceptionPlugins { get; private set; }

        #endregion InterceptionPlugins

        #region MarbleDiagramTemplateSelectorPlugin

        /// <summary>
        /// Gets the marble diagram template selector plugin.
        /// </summary>
        public static IEnumerable<IMarblePanelPlugin> MarbleDiagramTemplateSelectorPlugin { get; private set; }

        #endregion MarbleDiagramTemplateSelectorPlugin

        #region LoadResourcesPlugins

        /// <summary>
        /// Gets the load resources plug-in.
        /// </summary>
        public static IEnumerable<ILoadResourcesPlugin> LoadResourcesPlugins { get; private set; }

        #endregion LoadResourcesPlugins

        #region TabTemplateSelectorPlugins

        /// <summary>
        /// Gets the tab template selector plugins.
        /// </summary>
        public static ITabPlugin[] TabTemplateSelectorPlugins { get; private set; }

        #endregion TabTemplateSelectorPlugins

        #region DiagramImageMappersPlugins

        /// <summary>
        /// Gets the diagram image mapping plug-ins.
        /// </summary>
        public static IEnumerable<ILineHeaderImagePlugin> DiagramImageMappersPlugins { get; private set; }

        #endregion DiagramImageMappersPlugins

        #region MarbleTemplateSelectorPlugin

        /// <summary>
        /// Gets the marble template selector plug-in.
        /// </summary>
        public static IEnumerable<IMarbleItemPlugin> MarbleTemplateSelectorPlugin { get; private set; }

        #endregion MarbleTemplateSelectorPlugin

        #region ViewModel

        /// <summary>
        /// Gets the view model.
        /// </summary>
        public static MarbleDiagramsTabsViewModel ViewModel { get; private set; }

        #endregion ViewModel

        #region AddMarbleItem

        /// <summary>
        /// Add item into the item flow
        /// </summary>
        /// <param name="items"></param>
        public static void AddMarbleItem(MarbleBase[] items)
        {
            _subject.OnNext(items);
        }

        #endregion AddMarbleItem

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

        #region OnNewItem

        /// <summary>
        /// consume the marble item stream each time new marble item arrive
        /// </summary>
        /// <param name="newGroup"></param>
        public static void OnNewItem(MarbleBase[] items)
        {
            foreach (var item in items)
            {
                ViewModel.AppendMarble(new MarbleItemViewModel(item));
            }
        }

        #endregion OnNewItem
        
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