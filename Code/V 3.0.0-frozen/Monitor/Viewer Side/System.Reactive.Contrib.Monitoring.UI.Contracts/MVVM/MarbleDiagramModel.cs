#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using media = System.Windows.Media;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Diagram view model represent the actual marble on the diagram
    /// </summary>
    public class MarbleDiagramModel : 
        IMarbleDiagramContext, INotifyPropertyChanged,
        IDisposable
    {
        #region Constants

        private const int ITEM_SIZE = 28;
        internal const int DIAGRAM_WIDTH_CORRECTION = 20;
        internal const int MIN_DIAGRAM_WIDTH = 50;

        #endregion Constants

        #region Private / Protected Fields

        private static readonly ConcurrentDictionary<string, SolidColorBrush> _colors = new ConcurrentDictionary<string, SolidColorBrush>();
        private readonly ThreadLocal<Random> _rnd = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
        private double _marbleInitTimeInSeconds = -1;
        private int _sequence = -1;

        private Command _refreshCommand;
        private Command _globalTimeCommand;
        private Command _privateTimeCommand;
        private Command _sequenceCommand;
        private Command _showActions;
        private Command _hideActions;

        #endregion Private / Protected Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleDiagramModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="indexOrder">The index order.</param>
        /// <param name="mainContext">The main context.</param>
        public MarbleDiagramModel(string name, double indexOrder, IMainContext mainContext)
        {
            //RawItems = new MarbleCollection<MarbleItemViewModel>();
            RawItems = new MarbleCollection<MarbleItemViewModel>();
            //Items = (CollectionView)CollectionViewSource.GetDefaultView(RawItems);
            Name = name;
            IndexOrder = indexOrder;
            MainContext = mainContext;
            MainContext.PropertyChanged += RaiseDiagramWidthChanged;

            SolidColorBrush color = _colors.GetOrAdd(name, n => CreateColor());
            Color = color;
            PositioningStrategies = Enum.GetValues(typeof(MarblePositioningStrategy)) as MarblePositioningStrategy[];
            PositioningStrategy = MarblePositioningStrategy.GlobalTime;
            Height = ITEM_SIZE + 5;

            #region Actions

            #region _refreshCommand = new Command(...)

            _refreshCommand = new Command(param =>
            {
                Items.Refresh();
                ActionBoardVisibility = Visibility.Collapsed;
            });

            #endregion _refreshCommand = new Command(...)

            #region _globalTimeCommand = new Command(...)

            _globalTimeCommand = new Command(param =>
                {
                    PositioningStrategy = MarblePositioningStrategy.GlobalTime;
                    ActionBoardVisibility = Visibility.Collapsed;
                });

            #endregion _globalTimeCommand = new Command(...)

            #region _privateTimeCommand = new Command(...)

            _privateTimeCommand = new Command(param =>
                {
                    PositioningStrategy = MarblePositioningStrategy.PrivateTime;
                    ActionBoardVisibility = Visibility.Collapsed;
                });

            #endregion _privateTimeCommand = new Command(...)

            #region _sequenceCommand = new Command(...)

            _sequenceCommand = new Command(param =>
                {
                    PositioningStrategy = MarblePositioningStrategy.Sequence;
                    ActionBoardVisibility = Visibility.Collapsed;
                });

            #endregion _sequenceCommand = new Command(...)

            #region _showActions = new Command(...)

            _showActions = new Command(param =>
                {
                    if (ActionBoardVisibility == Visibility.Visible)
                        ActionBoardVisibility = Visibility.Collapsed;
                    else
                        ActionBoardVisibility = Visibility.Visible;
                });

            #endregion _showActions = new Command(...)

            #region _hideActions = new Command(...)

            _hideActions = new Command(param =>
                {
                    ActionBoardVisibility = Visibility.Collapsed;
                });

            #endregion _hideActions = new Command(...)

            #endregion Actions
        }

        #endregion Constructors

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        #endregion // INotifyPropertyChanged Members

        #region Properties

        #region Name

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        #endregion Name

        #region Color

        /// <summary>
        /// Gets the color.
        /// </summary>
        public SolidColorBrush Color { get; private set; }

        #endregion Color

        #region IndexOrder

        /// <summary>
        /// Gets the index order.
        /// </summary>
        public double IndexOrder { get; private set; }

        #endregion IndexOrder

        #region Size

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size
        {
            get
            {
                return ITEM_SIZE;
            }
        }

        #endregion Size

        #region Height

        /// <summary>
        /// Gets or sets the diagram height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; set; }

        #endregion Height

        #region PositioningStrategies

        /// <summary>
        /// Gets or sets the marble positioning strategy.
        /// </summary>
        /// <value></value>
        public MarblePositioningStrategy[] PositioningStrategies { get; set; }

        #endregion PositioningStrategies

        #region PositioningStrategy

        private MarblePositioningStrategy _positioningStrategy;

        /// <summary>
        /// Gets or sets the positioning strategy.
        /// </summary>
        /// <value>
        /// The positioning strategy.
        /// </value>
        public MarblePositioningStrategy PositioningStrategy
        {
            get
            {
                return _positioningStrategy;
            }
            set
            {
                _positioningStrategy = value;
                PropertyChanged(this, new PropertyChangedEventArgs("PositioningStrategy"));
                Items.Refresh();
            }
        }

        #endregion PositioningStrategy

        #region RawItems

        /// <summary>
        /// Gets or sets the raw items.
        /// </summary>
        /// <value>
        /// The raw items.
        /// </value>
        public MarbleCollection<MarbleItemViewModel> RawItems { get; set; }

        #endregion // RawItems

        #region Items

        /// <summary>
        /// Gets the items.
        /// </summary>
        public CollectionView Items { get { return RawItems.View; } }

        #endregion Items

        #region Unit

        /// <summary>
        /// Gets or sets the unit (for the timeline).
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public MarbleUnit Unit { get; set; }

        #endregion Unit

        #region DiagramWidth

        /// <summary>
        /// Gets the width of the diagram.
        /// </summary>
        /// <value>
        /// The width of the diagram.
        /// </value>
        public double DiagramWidth 
        {
            get 
            {
                return MainContext.DiagramWidth < MIN_DIAGRAM_WIDTH ? MIN_DIAGRAM_WIDTH : MainContext.DiagramWidth /* - DIAGRAM_WIDTH_CORRECTION */;
            }
        }

        #endregion DiagramWidth

        #region MainContext

        /// <summary>
        /// Gets the main context (tab level (diagrams)).
        /// </summary>
        public IMainContext MainContext { get; private set; }

        #endregion MainContext

        #region ActionBoardVisibility

        private Visibility _actionBoardVisibility = Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the action board visibility.
        /// </summary>
        /// <value>
        /// The action board visibility.
        /// </value>
        public Visibility ActionBoardVisibility
        {
            get { return _actionBoardVisibility; }
            set
            {
                _actionBoardVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ActionBoardVisibility"));
            }
        }

        #endregion ActionBoardVisibility

        #endregion Properties

        #region Commands

        #region RefreshCommand

        /// <summary>
        /// Gets the refresh.
        /// </summary>
        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand;
            }
        }

        #endregion RefreshCommand

        #region PrivateTimeCommand

        /// <summary>
        /// private time Command
        /// </summary>
        public ICommand PrivateTimeCommand
        {
            get
            {
                return _privateTimeCommand;
            }
        }

        #endregion PrivateTimeCommand

        #region GlobalTimeCommand

        /// <summary>
        /// global time Command
        /// </summary>
        public ICommand GlobalTimeCommand
        {
            get
            {
                return _globalTimeCommand;
            }
        }

        #endregion GlobalTimeCommand

        #region SequenceCommand

        /// <summary>
        /// Sequence Command.
        /// </summary>
        public ICommand SequenceCommand
        {
            get
            {
                return _sequenceCommand;
            }
        }

        #endregion SequenceCommand

        #region ShowActions

        /// <summary>
        /// Show actions.
        /// </summary>
        public ICommand ShowActions
        {
            get
            {
                return _showActions;
            }
        }

        #endregion ShowActions

        #region HideActions

        /// <summary>
        /// Hide actions.
        /// </summary>
        public ICommand HideActions
        {
            get
            {
                return _hideActions;
            }
        }

        #endregion HideActions

        #endregion Commands

        #region Methods

        #region GetLocalTimeInSeconds

        /// <summary>
        /// Gets the relative time in seconds (per marble diagram).
        /// </summary>
        /// <value>
        /// The item time in seconds.
        /// </value>
        public double GetLocalTimeInSeconds(double itemTotalSecond)
        {
            double initTime = _marbleInitTimeInSeconds;
            if (initTime == -1)
                initTime = Interlocked.CompareExchange(ref _marbleInitTimeInSeconds, itemTotalSecond, -1);

            return itemTotalSecond - initTime;
        }

        #endregion GetLocalTimeInSeconds

        #region AppendMarble

        /// <summary>
        /// Adds the specified item wrapper.
        /// </summary>
        /// <param name="itemWrapper">The item wrapper.</param>
        public void AppendMarble(MarbleItemViewModel itemWrapper)
        {
            #region Validation

            if (itemWrapper == null)
            {
                Debug.Fail("itemWrapper == null");
                return;
            }

            #endregion Validation

            int sequence = Interlocked.Increment(ref _sequence);
            itemWrapper.Sequence = sequence;
            RawItems.AddAsync(itemWrapper);
        }

        #endregion AppendMarble

        #region CreateColor

        /// <summary>
        /// Creates the color.
        /// </summary>
        /// <returns></returns>
        private SolidColorBrush CreateColor()
        {
            byte r = (byte)_rnd.Value.Next(255);
            byte g = (byte)_rnd.Value.Next(255);
            byte b = (byte)_rnd.Value.Next(255);

            var brush = new SolidColorBrush(
            media.Color.FromArgb(255, r, g, b));
            brush.Freeze();
            return brush;
        }

        #endregion // CreateColor

        #region RaiseDiagramWidthChanged

        /// <summary>
        /// Raises the diagram width changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void RaiseDiagramWidthChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DiagramWidth")
                PropertyChanged(this, new PropertyChangedEventArgs("DiagramWidth"));
        }

        #endregion // RaiseDiagramWidthChanged

        #endregion Methods

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="MarbleDiagramModel"/> is reclaimed by garbage collection.
        /// </summary>
        ~MarbleDiagramModel()
        { 
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposed)
        {
            _rnd.Dispose();
            MainContext.PropertyChanged -= RaiseDiagramWidthChanged;
        }

        #endregion // Dispose Pattern
    }
}