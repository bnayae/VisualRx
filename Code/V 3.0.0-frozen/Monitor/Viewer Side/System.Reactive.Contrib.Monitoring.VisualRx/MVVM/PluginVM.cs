using System.ComponentModel;
using System.Reactive.Contrib.Monitoring.UI.Contracts;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Plug-in view model
    /// </summary>
    public class PluginVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        #region Ctor

        public PluginVM(IPluginBundle plugin)
        {
            Plugin = plugin;
            var configuration = ConfigurationController.Value;
            var activation = configuration.PluginsActivation;
            Enabled = activation.GetOrAdd(plugin.Id, true);
        }

        #endregion // Ctor

        #region Plug-in

        /// <summary>
        /// Gets the plug-in.
        /// </summary>
        public IPluginBundle Plugin { get; private set; }

        #endregion // Plug-in

        #region Enabled

        private bool _enabled;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PluginVM"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                var configuration = ConfigurationController.Value;
                var activation = configuration.PluginsActivation;
                activation[Plugin.Id] = _enabled;
                PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
            }
        }

        #endregion // Enabled
    }
}