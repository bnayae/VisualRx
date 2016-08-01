#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#endregion // Using

namespace VisualRxDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var catalog = new AssemblyCatalog(typeof(MainWindow).Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            DataContext = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            VisualRxSettings.ClearFilters();

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create());

            VisualRxInitResult infos = info.Result;
            Trace.WriteLine(infos);
        }

        [ImportMany]
        private IScenario[] Scenarios { get; set; }
        public IEnumerable<IScenario> OrderedScenarios { get { return Scenarios.OrderBy(item => item.Order); } }
        public IScenario Current { get; set; }
    }
}
