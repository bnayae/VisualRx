
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;

namespace System.Reactive.Contrib.Monitoring.UI
{
    public class TreeItem : NotifiableObject
    {
        #region Properties

        public bool IsFullyLoaded { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged(() => Name);
            }
        }

        public TreeItem Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
                NotifyPropertyChanged(() => Parent);
            }
        }

        public ObservableCollection<TreeItem> Childs
        {
            get
            {
                return childs;
            }
            set
            {
                childs = value;
                NotifyPropertyChanged(() => Childs);
            }
        }

        #endregion

        public TreeItem(string name, TreeItem parent)
        {
            Name = name;
            IsFullyLoaded = false;
            Parent = parent;
            Childs = new ObservableCollection<TreeItem>();
        }

        public string GetFullPath()
        {
            Stack<string> stack = new Stack<string>();

            var ti = this;

            while (ti.Parent != null)
            {
                stack.Push(ti.Name);
                ti = ti.Parent;
            }

            string path = stack.Pop();

            while (stack.Count > 0)
            {
                path = Path.Combine(path, stack.Pop());
            }

            return path;
        }

        #region Private fields

        private string name;
        private TreeItem parent;
        private ObservableCollection<TreeItem> childs;

        #endregion
    }

    public class DriveTreeItem : TreeItem
    {
        public DriveType DriveType { get; set; }

        public DriveTreeItem(string name, DriveType driveType, TreeItem parent)
            : base(name, parent)
        {
            DriveType = driveType;
        }
    }

    public class NotifiableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;
            OnPropertyChanged(memberExpression.Member.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
