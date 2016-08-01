using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Configuration view model
    /// </summary>
    public class ConfigurationVM : INotifyPropertyChanged
    {
        private readonly ICommand _addDiscoveryPathCommand;
        private static readonly Lazy<ConfigurationVM> _instance = new Lazy<ConfigurationVM>(() => new ConfigurationVM());
        #region Ctor

        private ConfigurationVM()
        {
            ConfigurationController.ValueChanged += (s, e) => PropertyChanged(this, new PropertyChangedEventArgs("Model"));
            _addDiscoveryPathCommand = new Command(state =>
                {
                    DiscoveryPath path = new DiscoveryPath();
                    Model.PluginDiscoveryPaths.Add(path);
                    PropertyChanged(this, new PropertyChangedEventArgs("Model"));
                    PluginDiscoveryPathsSelected = path;
                }, state => true);

            Plugins = MarbleController.Plugins;
        }

        #endregion // Ctor

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        /// <summary>
        /// Gets a singleton instance.
        /// </summary>
        public static ConfigurationVM Instance { get { return _instance.Value; } }

        #region Model

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public Configuration Model { get { return ConfigurationController.Value; } }

        #endregion // Model

        #region AddDiscoveryPathCommand

        /// <summary>
        /// Gets the add discovery path.
        /// </summary>
        public ICommand AddDiscoveryPathCommand { get { return _addDiscoveryPathCommand; } }

        #endregion // AddDiscoveryPathCommand

        #region PluginDiscoveryPathsSelected

        private DiscoveryPath _pluginDiscoveryPathsSelected;
        /// <summary>
        /// Gets or sets the index of the plug-in discovery paths selected.
        /// </summary>
        /// <value>
        /// The index of the plug-in discovery paths selected.
        /// </value>
        public DiscoveryPath PluginDiscoveryPathsSelected
        {
            get
            {
                return _pluginDiscoveryPathsSelected;
            }
            set
            {
                _pluginDiscoveryPathsSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs("PluginDiscoveryPathsSelected"));
            }
        }

        #endregion // PluginDiscoveryPathsSelected

        #region Plug-ins

        /// <summary>
        /// Gets or sets the plug-ins activation setting.
        /// </summary>
        /// <value>
        /// The plug-ins activation.
        /// </value>
        public PluginVM[] Plugins { get; private set; }

        #endregion // Plug-ins
    }
}