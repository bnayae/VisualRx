#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Contrib.Monitoring.UI.Properties;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Tab view model
    /// </summary>
    public sealed class MarbleDiagramsTabsViewModel : IViewModel, INotifyPropertyChanged, IDisposable
    {
        #region Constants

        private static readonly int STARTUP_WIDTH = (int)SystemParameters.PrimaryScreenWidth - 200;
        private const string PAUSE_TEXT = "Pause";
        private const string RESUME_TEXT = "Resume";
        public const int DEFAULT_SAMPLE_WIDTH = 400;

        #endregion Constants

        #region Private / Protected Fields

        private ConcurrentDictionary<string, MarbleDiagramsViewModel> _diagramsByKeywords =
            new ConcurrentDictionary<string, MarbleDiagramsViewModel>();

        private string _pauseState = PAUSE_TEXT;
        private ISubject<MarbleItemViewModel> _pauseSubject;

        private Command _clearCommand;

        #endregion Private / Protected Fields

        #region Constructors

        public MarbleDiagramsTabsViewModel()
        {
            var catalog = new DirectoryCatalog(".", "*.dll");
            var container = new CompositionContainer(catalog);
            _allSamples = container.GetExportedValues<ISample>() ?? Enumerable.Empty<ISample>();
            var localCatalog = new TypeCatalog(typeof(MainTabPlugin));

            Tabs = new MarbleCollection<ITabModel>();

            foreach (var plugin in MarbleController.TabTemplateSelectorPlugins)
            {
                plugin.TabModel.SetViewModel(this);
                if (Selected == null || plugin.TabModel.Keyword == Constants.MAIN_DIAGRAM_KEY)
                    Selected = plugin.TabModel;
                Tabs.AddSync(plugin.TabModel);
            }
            Width = STARTUP_WIDTH;

            #region _clearCommand = new Command(...)

            _clearCommand = new Command(param =>
            {
                Clear();
            });

            #endregion _clearCommand = new Command(...)

            _sampleFilter.Throttle(TimeSpan.FromSeconds(0.5))
                         .ObserveOn(SynchronizationContext.Current)
                         .Subscribe(m => PropertyChanged(this, new PropertyChangedEventArgs(nameof(Samples))));
        }

        #endregion Constructors

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        #region Properties

        #region Tabs

        /// <summary>
        /// Gets the tabs.
        /// </summary>
        public MarbleCollection<ITabModel> Tabs { get; private set; }

        #endregion Tabs

        #region SamplesFilter

        private BehaviorSubject<string> _sampleFilter = new BehaviorSubject<string>(null);

        public string SamplesFilter
        {
            get { return _sampleFilter.Value; }
            set
            {
                _sampleFilter.OnNext(value);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SamplesFilter)));
            }
        }

        #endregion // SamplesFilter

        private IEnumerable<ISample> _allSamples;

        #region Samples

        public IEnumerable<ISample> Samples =>
            _allSamples.Where(m => string.IsNullOrEmpty(SamplesFilter) ||
                            m.Title.IndexOf(SamplesFilter, StringComparison.OrdinalIgnoreCase) != -1)
                       .OrderBy(m => m.Order);

        #endregion // Samples

        #region CurrentSample

        private ISample _currentSample;
        /// <summary>
        /// Gets or sets the current sample.
        /// </summary>
        public ISample CurrentSample
        {
            get { return _currentSample; }
            set
            {
                _currentSample = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentSample)));
            }
        }

        #endregion // CurrentSample

        #region Selected

        /// <summary>
        /// Gets or sets the selected.
        /// </summary>
        /// <value>
        /// The selected.
        /// </value>
        public ITabModel Selected { get; set; }

        #endregion Selected

        #region AnimateElement

        /// <summary>
        /// Gets or sets the animate element.
        /// </summary>
        /// <value>
        /// The animate element.
        /// </value>
        public FrameworkElement AnimateElement { get; set; }

        #endregion AnimateElement

        #region Width

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; set; }

        #endregion Width

        #region SamplesWidth

        private int _samplesWidth = 0;

        /// <summary>
        /// Gets or sets the width of the samples.
        /// </summary>
        public int SamplesWidth
        {
            get { return _samplesWidth; }
            set
            {
                _samplesWidth = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SamplesWidth)));
            }
        }

        #endregion SamplesWidth

        #region Pinged

        private bool _pinged = true;

        /// <summary>
        /// Gets or sets a value indicating whether remote monitor has discover the viewer.
        /// User for binding animation
        /// </summary>
        /// <value>
        ///   <c>true</c> if pinged; otherwise, <c>false</c>.
        /// </value>
        public bool Pinged
        {
            get
            {
                return _pinged;
            }
            set
            {
                _pinged = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Pinged"));
            }
        }

        #endregion Pinged

        #region PauseState

        /// <summary>
        /// Gets or sets the state of the pause.
        /// </summary>
        /// <value>
        /// The state of the pause.
        /// </value>
        public string PauseState
        {
            get
            {
                return _pauseState;
            }
            set
            {
                _pauseState = value;
                PropertyChanged(this, new PropertyChangedEventArgs("PauseState"));
            }
        }

        #endregion PauseState

        #region Version

        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        /// <value>
        /// The Version.
        /// </value>
        public string Version { get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); } }

        #endregion Version

        #endregion Properties

        #region Methods

        #region StartAnimation

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void StartAnimation()
        {
            Pinged = false;
            Pinged = true;
            //if (Settings.Default.BeepWhenDiscovered)
            //    Console.Beep(250, 150);
        }

        #endregion StartAnimation

        #region TogglePause

        /// <summary>
        /// Toggles the pause.
        /// </summary>
        public void TogglePause()
        {
            try
            {
                if (PauseState == PAUSE_TEXT)
                {
                    _pauseSubject = new ReplaySubject<MarbleItemViewModel>();
                    PauseState = RESUME_TEXT;
                }
                else
                {
                    PauseState = PAUSE_TEXT;
                    _pauseSubject
                        .ObserveOn(SynchronizationContext.Current)
                        .Subscribe(AppendMarble);
                }
            }
            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Pause toggle filed: {0}", ex);
            }
        }

        #endregion TogglePause

        #region AppendMarble

        /// <summary>
        /// Appends the marble and create new diagrams tab per keyword
        /// </summary>
        public void AppendMarble(MarbleItemViewModel value)
        {
            #region Pause Switching

            if (PauseState == RESUME_TEXT)
            {
                // route the traffic to a replay subject during the pause stage
                _pauseSubject.OnNext(value);
                return;
            }

            #endregion Pause Switching

            #region TabTemplateSelectorPlugins

            foreach (ITabPlugin tab in MarbleController.TabTemplateSelectorPlugins)
            {
                try
                {
                    tab.TabModel.AppendMarble(value);
                }
                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("TabTemplateSelectorPlugin filed: {0}", ex);
                }

                #endregion Exception Handling
            }

            #endregion TabTemplateSelectorPlugins

            foreach (var keyword in value.Item.Keywords)
            {
                #region Add MarbleDiagramsViewModel if not exists

                var diagrams = _diagramsByKeywords.GetOrAdd(keyword, key =>
                {
                    var dgm = new MarbleDiagramsViewModel(keyword, this);
                    Tabs.AddAsync(dgm);
                    return dgm;
                });

                #endregion Add MarbleDiagramsViewModel if not exists

                diagrams.AppendMarble(value);
            }

            #region InterceptionPlugins

            foreach (var plugin in MarbleController.InterceptionPlugins)
            {
                try
                {
                    plugin.AppendMarble(value.Item);
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    TraceSourceMonitorHelper.Error("InterceptionPlugin filed: {0}", ex);
                }

                #endregion Exception Handling
            }

            #endregion InterceptionPlugins
        }

        #endregion AppendMarble

        #region Clear

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Selected = null;
            int tabPluginCount = MarbleController.TabTemplateSelectorPlugins.Length;
            for (int i = Tabs.Count - 1; i >= tabPluginCount; i--)
            {
                Tabs.RemoveAt(i);
            }

            foreach (ITabModel tab in MarbleController.ViewModel.Tabs)
            {
                tab.Clear();
            }

            _diagramsByKeywords.Clear();
        }

        #endregion Clear

        #endregion Methods

        #region Actions

        #region ClearCommand

        /// <summary>
        /// Gets the clear Command.
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand;
            }
        }

        #endregion ClearCommand

        #endregion Actions
        	
		#region Dispose Pattern 
    
		#region Dispose

		#region Documentation
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		#endregion
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

		#endregion // Dispose

        #region Dispose

        #region Documentation
        /// <summary>
        ///  Enforce the dispose
        /// </summary>
        /// <param name="disposing"></param>
        #endregion // Documentation
        private void Dispose (bool disposing)
        {            
            try
            {
                var pauseSubject = _pauseSubject as IDisposable;
                if (pauseSubject != null)
                    pauseSubject.Dispose();
            }
            #region Exception Handling

            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("Dispose filed: {0}", ex);
            }

            #endregion // Exception Handling
        }

        #endregion // Dispose
    
		#region Destructor

		#region Documentation
		/// <summary>
		/// Destructor
		/// </summary>
		#endregion
		~MarbleDiagramsTabsViewModel ()
		{
			Dispose (false);
		}

		#endregion // Destructor


		#endregion // Dispose Pattern 
    }
}