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
using System.ComponentModel;
using System.Collections;

#endregion Using

// TODO: check for hierarchic

namespace System.Reactive.Contrib.Monitoring.UI
{
    public class VMDataStream : IVMDataStream, INotifyPropertyChanged
    {
        private string _fullName;

        public VMDataStream(string name, string fullName, string[] keywords, ListCollectionView view)
        {
            Name = name;
            FullName = fullName;
            Keywords = keywords;
            Children = new List<IVMDataStream>();

            Items = view;
        }

        public string Name { get; private set; }
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string[] Keywords { get; private set; }

        public IList<IVMDataStream> Children { get; private set; }

        public ListCollectionView Items { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}