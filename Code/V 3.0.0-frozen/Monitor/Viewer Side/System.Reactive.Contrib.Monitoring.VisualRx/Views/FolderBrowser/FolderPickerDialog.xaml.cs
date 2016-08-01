using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Interaction logic for FolderPickerDialog.xaml
    /// </summary>
    public partial class FolderPickerDialog : Window
    {
        #region Dependency properties

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(FolderPickerDialog));

        public Style ItemContainerStyle
        {
            get
            {
                return (Style)GetValue(ItemContainerStyleProperty);
            }
            set
            {
                SetValue(ItemContainerStyleProperty, value);
            }
        }

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FolderPickerDialog;
            if (control != null)
            {
                control.ItemContainerStyle = e.NewValue as Style;
            }
        }

        #endregion

        public string SelectedPath { get; private set; }

        public string InitialPath
        {
            get
            {
                return FolderPickerControl.InitialPath;
            }
            set
            {
                FolderPickerControl.InitialPath = value;
            }
        }

        public FolderPickerDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = FolderPickerControl.SelectedPath;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComponentDispatcher.IsThreadModal)
            {
                DialogResult = false;
            }
            else
            {
                Close();
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPickerControl.CreateNewFolder();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPickerControl.RefreshTree();
        }

        #region OnDragHandler

        /// <summary>
        /// Called when [drag handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnDragHandler(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                this.Top = 1;
                WindowState = WindowState.Normal;
            }
            this.DragMove();
        }

        #endregion // OnDragHandler
    }
}
