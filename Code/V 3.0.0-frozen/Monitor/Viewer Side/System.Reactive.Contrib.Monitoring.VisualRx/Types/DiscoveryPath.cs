#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.IO;
using System.ComponentModel;
using System.Windows;

#endregion // Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Use to represent the discovery path in a binding friendly way.
    /// </summary>
    [DataContract]
    public class DiscoveryPath: INotifyPropertyChanged
    {
        private ICommand _removeCommand;
        private ICommand _browseCommand;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryPath"/> class.
        /// </summary>
        /// <param name="removeCommand">The remove command.</param>
        public DiscoveryPath()
        {
            Id = Guid.NewGuid();
            _path = string.Empty;
            _removeCommand = new Command(Remove, state => true);
            _browseCommand = new Command(Browse, state => true);
        }

        #endregion // Ctor


        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        private Guid Id { get; set; }

        #region Path

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string _path;
        [DataMember]
        public string Path 
        {
            get { return _path; }
            set
            {
                if (_path == value)
                    return;
                _path = value;
                if(PropertyChanged != null)
                    PropertyChanged (this, new PropertyChangedEventArgs("Path"));
            }
        }

        #endregion // Path

        #region Remove

        /// <summary>
        /// Removes the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void Remove(object state)
        {
            ConfigurationController.Value.PluginDiscoveryPaths.Remove(this);
        }

        #endregion // Remove

        #region Browse

        /// <summary>
        /// Browse directory.
        /// </summary>
        /// <param name="state">The state.</param>
        private void Browse(object state)
        {
            try
            {
                string root = Directory.GetCurrentDirectory();
                var dir = string.IsNullOrWhiteSpace(_path) ? root : _path;
                dir = System.IO.Path.GetFullPath(dir);
                var folderPicker = new FolderPickerDialog { InitialPath = dir, ShowInTaskbar = false };
                if (folderPicker.ShowDialog() == true)
                {
                    string path = folderPicker.SelectedPath;
                    if (path.StartsWith(dir, StringComparison.InvariantCultureIgnoreCase) &&
                        path.Length > root.Length)
                    {
                        path = path.Substring(root.Length + 1);
                    }
                    Path = path;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion // Browse

        #region RemoveCommand

        /// <summary>
        /// Gets the remove command.
        /// </summary>
        public ICommand RemoveCommand { get { return _removeCommand; } }

        #endregion // RemoveCommand

        #region BrowseCommand

        /// <summary>
        /// Gets the browse command.
        /// </summary>
        public ICommand BrowseCommand { get { return _browseCommand; } }

        #endregion // BrowseCommand

        #region Equals

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var item = obj as DiscoveryPath;
            if (item == null)
                return false;
            return Id == item.Id;
        }

        #endregion // Equals

        #region GetHashCode

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion // GetHashCode

        #region Interception Points

        #region OnDeserializedMethod

        /// <summary>
        /// Called when [deserialized method].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            _removeCommand = new Command(Remove, state => true);
        }

        #endregion // OnDeserializedMethod

        #endregion // Interception Points
    }
}