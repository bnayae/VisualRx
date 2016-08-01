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
using System.ComponentModel.Composition;
using System.Windows.Data;

#endregion Using

// TODO: Handle settings: clear after minutes, plug-ins,

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// The main view model for RX 
    /// </summary>
    internal class ViewModel: IViewModel
    {
        #region Private / Protected Fields

        private readonly BulkObservableCollection<MarbleBase> _flatData = new BulkObservableCollection<MarbleBase>();
        private readonly BulkObservableCollection<string> _keywords = new BulkObservableCollection<string>();
        private readonly BulkObservableCollection<IVMDataStream> _hierarchic = new BulkObservableCollection<IVMDataStream>();

        private readonly Subject<MarbleBase[]> _serviceStream = new Subject<MarbleBase[]>();
        private readonly ItemNameEqualityComparer _itemNameComparer = new ItemNameEqualityComparer();

        #endregion // Private / Protected Fields

        #region Ctor

        public ViewModel()
        {
            // TODO: should re-subscribe on clear
            DistinctKeyWordStream.Subscribe(k => _keywords.Add(k));
            // TODO: should re-subscribe on clear
            FirstItemStream
                //.ObserveOn(DispatcherScheduler.Current)
                .Subscribe(AppendStream);
        }

        #endregion // Ctor

        #region AddRange

        /// <summary>
        /// the view model data entry point
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(MarbleBase[] items)
        {
            _serviceStream.OnNext(items); // used to add new streams
            _flatData.AddRange(items); // the actual data (the data will be filtered by each stream)
        }

        #endregion // AddRange

        #region AppendStream

        /// <summary>
        /// Append new stream to the stream hierarchic.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AppendStream(MarbleBase item)
        {
            string fullName = item.Name;

            #region Validation

            if (string.IsNullOrEmpty(fullName))
                return; // TODO: error log

            #endregion // Validation

            string[] path = fullName.Split(new []{"."}, StringSplitOptions.RemoveEmptyEntries);
            IVMDataStream parent = _hierarchic.FirstOrDefault(node => node.Name == path[0]);
            var request = new CreateStreamVMRequest(item, parent, path.ToArray(), 1);
            CreateStreamVMRecursive(request);
        }

        #endregion // AppendStream

        #region CreateStreamVMRecursive

        /// <summary>
        /// Creates the stream VM recursively.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="path">The path.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private void CreateStreamVMRecursive(CreateStreamVMRequest request)
        {
            IVMDataStream vmStream = null;
            if (request.Parent != null)
                vmStream = request.Parent.Children.FirstOrDefault(node => node.Name == request.FullName);

            if (vmStream != null)
            {
                // TODO: add keywords to existing paths
                if (request.IsLastLevel)
                    return;
            }
            else
            { 
                ListCollectionView view = new ListCollectionView(_flatData);
                string fullName = request.FullName;
                view.Filter = obj => ((MarbleBase)obj).Name.StartsWith(fullName); // TBD: whether intermediate level should have an active ListCollectionView?

                string[] keywords = request.Item.Keywords; 
                string name = request.Path[request.Level - 1];
                vmStream = new VMDataStream(name, fullName, keywords, view);

                if (request.Parent == null)
                    _hierarchic.Add(vmStream);
                if (request.IsLastLevel)
                    return;
            }

            request.IncreamentLevel(vmStream);
            CreateStreamVMRecursive(request);
        }

        #endregion // CreateStreamVMRecursive

        #region HierarchicRoot

        /// <summary>
        /// Gets an hierarchic root of the marble diagrams.
        /// </summary>
        /// <value>
        /// The hierarchic.
        /// </value>
        public IEnumerable<IVMDataStream> HierarchicRoot { get { return _hierarchic; } }

        #endregion // HierarchicRoot

        #region FlatData
        
        /// <summary>
        /// Gets a flat data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public IEnumerable<MarbleBase> FlatData { get { return _flatData; } }

        #endregion // FlatData

        #region AllKeywords

        /// <summary>
        /// Gets all the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        public IEnumerable<string> AllKeywords { get { return _keywords; } }

        #endregion // AllKeywords

        #region FirstItemStream

        /// <summary>
        /// Gets the first item stream.
        /// </summary>
        /// <value>
        /// The first item stream.
        /// </value>
        /// <remarks>
        /// Can use as a trigger to creation of Visual UI stream
        /// </remarks>
        private IObservable<MarbleBase> FirstItemStream 
        {
            get
            {
                var result = from items in _serviceStream
                             from item in items
                             select item;

                return result.Distinct(_itemNameComparer);
            }
        }

        #endregion // FirstItemStream

        #region DistinctKeyWordStream

        /// <summary>
        /// Gets the distinct key word stream.
        /// </summary>
        /// <value>
        /// The distinct key word stream.
        /// </value>
        private IObservable<string> DistinctKeyWordStream 
        {
            get
            {
                var result = from item in FirstItemStream
                             let keywords = item.Keywords.ToObservable()
                             from keyword in keywords
                             select keyword;

                return result.Distinct();
            }
        }

        #endregion // DistinctKeyWordStream

        #region Nested Types
        
		#region ItemNameEqualityComparer

        /// <summary>
        /// use as a better performance option than a delegate comparison
        /// </summary>
        private class ItemNameEqualityComparer : IEqualityComparer<MarbleBase>
        {
            public bool Equals(MarbleBase x, MarbleBase y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(MarbleBase obj)
            {
                return obj.Name.GetHashCode();
            }
        }

		#endregion // ItemNameEqualityComparer
        
		#region CreateStreamVMRequest

        /// <summary>
        /// Recursive stream creation request
        /// </summary>
        private class CreateStreamVMRequest
        {
            string _fullName;
            bool _isLastLevel;

            public CreateStreamVMRequest (MarbleBase item, IVMDataStream parent, string[] path, int level)
	        {
                Item = item;
                Parent = parent;
                Path = path;
                Level = level;
                _fullName = string.Join(".", Path, 0, Level);
                _isLastLevel = Path.Length == Level;
	        }

            public MarbleBase Item {get; private set;}
            public IVMDataStream Parent {get; private set;}
            public string[] Path {get; private set;}
            public int Level {get; private set;}

            public string FullName { get { return _fullName;} }            
            public bool IsLastLevel { get {return _isLastLevel;} }
            public void IncreamentLevel(IVMDataStream parent)
            {
                Level++;
                Parent = parent;
                _fullName = string.Join(".", Path, 0, Level);
                _isLastLevel = Path.Length == Level;
            }
        }

		#endregion // CreateStreamVMRequest

        #endregion // Nested Types
    }
}