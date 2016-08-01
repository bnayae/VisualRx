using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Interaction logic for FolderPicker.xaml
    /// </summary>
    public partial class FolderPickerControl : UserControl, INotifyPropertyChanged
    {
        #region Constants

        private const string EmptyItemName = "Empty";
        private const string NewFolderName = "New Folder";
        private const int MaxNewFolderSuffix = 10000;

        #endregion

        #region Properties

        public TreeItem Root
        {
            get
            {
                return root;
            }
            private set
            {
                root = value;
                NotifyPropertyChanged(() => Root);
            }
        }

        public TreeItem SelectedItem
        {
            get
            {
                return selectedItem;
            }
            private set
            {
                selectedItem = value;
                NotifyPropertyChanged(() => SelectedItem);
            }
        }

        public string SelectedPath { get; private set; }

        public string InitialPath
        {
            get
            {
                return initialPath;
            }
            set
            {
                initialPath = value;
                UpdateInitialPathUI();
            }
        }

        public Style ItemContainerStyle
        {
            get
            {
                return itemContainerStyle;
            }
            set
            {
                itemContainerStyle = value;
                OnPropertyChanged("ItemContainerStyle");
            }
        }

        #endregion

        public FolderPickerControl()
        {
            InitializeComponent();

            Init();
        }

        public void CreateNewFolder()
        {
            CreateNewFolderImpl(SelectedItem);
        }

        public void RefreshTree()
        {
            Root = null;
            Init();
        }

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

        #region Private methods

        private void Init()
        {
            root = new TreeItem("root", null);
            var systemDrives = DriveInfo.GetDrives();

            foreach (var sd in systemDrives)
            {
                var item = new DriveTreeItem(sd.Name, sd.DriveType, root);
                item.Childs.Add(new TreeItem(EmptyItemName, item));

                root.Childs.Add(item);
            }

            Root = root; // to notify UI
        }

        private void TreeView_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
            {
                SelectedItem = tvi.DataContext as TreeItem;
                SelectedPath = SelectedItem.GetFullPath();
            }
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            var treeItem = tvi.DataContext as TreeItem;

            if (treeItem != null)
            {
                if (!treeItem.IsFullyLoaded)
                {
                    treeItem.Childs.Clear();

                    string path = treeItem.GetFullPath();

                    DirectoryInfo dir = new DirectoryInfo(path);

                    try
                    {
                        var subDirs = dir.GetDirectories();
                        foreach (var sd in subDirs)
                        {
                            TreeItem item = new TreeItem(sd.Name, treeItem);
                            item.Childs.Add(new TreeItem(EmptyItemName, item));

                            treeItem.Childs.Add(item);
                        }
                    }
                    catch { }

                    treeItem.IsFullyLoaded = true;
                }
            }
            else
                throw new Exception();
        }

        private void UpdateInitialPathUI()
        {
            if (!Directory.Exists(InitialPath))
                return;

            var initialDir = new DirectoryInfo(InitialPath);

            if (!initialDir.Exists)
                return;

            var stack = TraverseUpToRoot(initialDir);
            var containerGenerator = TreeView.ItemContainerGenerator;
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            DirectoryInfo currentDir = null;
            var dirContainer = Root;

            AutoResetEvent waitEvent = new AutoResetEvent(true);

            Task processStackTask = Task.Factory.StartNew(() =>
                {
                    while (stack.Count > 0)
                    {
                        waitEvent.WaitOne();

                        currentDir = stack.Pop();

                        Task waitGeneratorTask = Task.Factory.StartNew(() =>
                        {
                            if (containerGenerator == null)
                                return;

                            while (containerGenerator.Status != GeneratorStatus.ContainersGenerated)
                                Thread.Sleep(50);
                        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

                        Task updateUiTask = waitGeneratorTask.ContinueWith((r) =>
                        {
                            try
                            {
                                var childItem = dirContainer.Childs.Where(c => string.Compare(c.Name, currentDir.Name, true) == 0).FirstOrDefault();
                                if (childItem != null)
                                {
                                    var tvi = containerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                                    dirContainer = tvi.DataContext as TreeItem;
                                    tvi.IsExpanded = true;

                                    tvi.Focus();

                                    containerGenerator = tvi.ItemContainerGenerator;
                                }
                            }
                            catch { }

                            waitEvent.Set();
                        }, uiContext);
                    }

                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private Stack<DirectoryInfo> TraverseUpToRoot(DirectoryInfo child)
        {
            if (child == null)
                return null;

            if (!child.Exists)
                return null;

            Stack<DirectoryInfo> queue = new Stack<DirectoryInfo>();
            queue.Push(child);
            DirectoryInfo ti = child.Parent;

            while (ti != null)
            {
                queue.Push(ti);
                ti = ti.Parent;
            }

            return queue;
        }

        private void CreateNewFolderImpl(TreeItem parent)
        {
            try
            {
                if (parent == null)
                    return;

                var parentPath = parent.GetFullPath();
                var newDirName = GenerateNewFolderName(parentPath);
                var newPath = Path.Combine(parentPath, newDirName);

                Directory.CreateDirectory(newPath);

                var childs = parent.Childs;
                var newChild = new TreeItem(newDirName, parent);
                childs.Add(newChild);
                parent.Childs = childs.OrderBy(c => c.Name).ToObservableCollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Can't create new folder. Error: {0}", ex.Message));
            }
        }

        private string GenerateNewFolderName(string parentPath)
        {
            string result = NewFolderName;

            if (Directory.Exists(Path.Combine(parentPath, result)))
            {
                for (int i = 1; i < MaxNewFolderSuffix; ++i)
                {
                    var nameWithIndex = String.Format( NewFolderName + " {0}", i);

                    if (!Directory.Exists(Path.Combine(parentPath, nameWithIndex)))
                    {
                        result = nameWithIndex;
                        break;
                    }
                }
            }

            return result;
        }

        private void CreateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item != null)
            {
                var context = item.DataContext as TreeItem;
                CreateNewFolderImpl(context);
            }
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = sender as MenuItem;
                if (item != null)
                {
                    var context = item.DataContext as TreeItem;
                    if (context != null && !(context is DriveTreeItem))
                    {
                        var dialog = new InputDialog()
                        {
                            Message = "New folder name:",
                            InputText = context.Name,
                            Title = String.Format("Do you really want to rename folder {0}?", context.Name)
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            var newFolderName = dialog.InputText;

                            /*
                             * Parent for context is always not null due to the fact
                             * that we don't allow to change the name of DriveTreeItem
                             */
                            var newFolderFullPath = Path.Combine(context.Parent.GetFullPath(), newFolderName);
                            if (Directory.Exists(newFolderFullPath))
                            {
                                MessageBox.Show(String.Format("Directory already exists: {0}", newFolderFullPath));
                            }
                            else
                            {
                                Directory.Move(context.GetFullPath(), newFolderFullPath);
                                context.Name = newFolderName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Can't rename folder. Error: {0}", ex.Message));
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = sender as MenuItem;
                if (item != null)
                {
                    var context = item.DataContext as TreeItem;
                    if (context != null && !(context is DriveTreeItem))
                    {
                        var confirmed =
                            MessageBox.Show(
                                String.Format("Do you really want to delete folder {0}?", context.Name),
                                "Confirm folder removal",
                                MessageBoxButton.YesNo);

                        if (confirmed == MessageBoxResult.Yes)
                        {
                            Directory.Delete(context.GetFullPath());
                            var parent = context.Parent;
                            parent.Childs.Remove(context);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Can't delete folder. Error: {0}", ex.Message));
            }
        }

        #endregion

        #region Private fields

        private TreeItem root;
        private TreeItem selectedItem;
        private string initialPath;
        private Style itemContainerStyle;

        #endregion
    }

    public class DriveIconConverter : IValueConverter
    {
        private static BitmapImage removable;
        private static BitmapImage drive;
        private static BitmapImage netDrive;
        private static BitmapImage cdrom;
        private static BitmapImage ram;
        private static BitmapImage folder;
        private readonly static string ASSEMBLY_NAME = Assembly.GetExecutingAssembly().GetName().Name;

        public DriveIconConverter()
        {
            if (removable == null)
                removable = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_8.ico");

            if (drive == null)
                drive = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_9.ico");

            if (netDrive == null)
                netDrive = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_10.ico");

            if (cdrom == null)
                cdrom = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_12.ico");

            if (ram == null)
                ram = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_303.ico");

            if (folder == null)
                folder = CreateImage("pack://application:,,,/" + ASSEMBLY_NAME + ";component/Images/FolderBrowser/shell32_264.ico");
        }

        private BitmapImage CreateImage(string uri)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(uri);
            img.EndInit();
            return img;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var treeItem = value as TreeItem;
            if (treeItem == null)
                throw new ArgumentException("Illegal item type");

            if (treeItem is DriveTreeItem)
            {
                DriveTreeItem driveItem = treeItem as DriveTreeItem;
                switch (driveItem.DriveType)
                {
                    case DriveType.CDRom:
                        return cdrom;
                    case DriveType.Fixed:
                        return drive;
                    case DriveType.Network:
                        return netDrive;
                    case DriveType.NoRootDirectory:
                        return drive;
                    case DriveType.Ram:
                        return ram;
                    case DriveType.Removable:
                        return removable;
                    case DriveType.Unknown:
                        return drive;
                }
            }
            else
            {
                return folder;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NullToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class LinqExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var result = new ObservableCollection<T>();

            foreach (var ci in source)
            {
                result.Add(ci);
            }

            return result;
        }
    }
}
