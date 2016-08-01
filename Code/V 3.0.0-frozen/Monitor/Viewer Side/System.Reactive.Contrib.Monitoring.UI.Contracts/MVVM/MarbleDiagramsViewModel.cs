#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// view model for single tab data
    /// </summary>
    public sealed class MarbleDiagramsViewModel : ITabModel, IMainContext, INotifyPropertyChanged
    {
        #region Constants

        private const int WIN_WIDTH_CORRECT = 10;
        private const int RESET_WIDTH = 100;
        private const string SCROLL_TO_END = "Scroll to end";
        private const string SCROLL_DEFAULT = "Default Scrolling";

        #endregion Constants

        #region Private / Protected Fields

        private double _diagramWidth;
        private MarbleUnit _unitSelected = MarbleUnit.Seconds;
        private double _diagramScale = 0.7;
        private bool _isScrollToEnd = false;
        private ScrollKind _scrollKindSelected = ScrollKind.ManualScroll;
        private double _canvasLeft = 0;
        private IViewModel _vm;
        private double _initTimeInSeconds = -1;

        private ConcurrentDictionary<string, MarbleDiagramModel> _marbleDiagrams = new ConcurrentDictionary<string, MarbleDiagramModel>();

        private Command _clearCommand;
        private Command _globalTimeCommand;
        private Command _privateTimeCommand;
        private Command _sequenceCommand;
        private Command _showAction;
        private Command _scrollAction;

        #endregion Private / Protected Fields

        #region event PropertyChangedEventHandler PropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        #endregion // event PropertyChangedEventHandler PropertyChanged

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleDiagramsViewModel"/> class.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="vm">The vm.</param>
        /// <param name="kind">The kind.</param>
        public MarbleDiagramsViewModel(string keyword, IViewModel vm, TabKind kind = TabKind.Marble)
        {
            _vm = vm;
            Diagrams = new MarbleCollection<MarbleDiagramModel>();
            SetDiagramOrdering();

            FlatItems = new MarbleCollection<MarbleBase>();
            //FlatItemsView = (CollectionView)CollectionViewSource.GetDefaultView(FlatItems);

            if (vm != null)
                _diagramWidth = _vm.Width - WIN_WIDTH_CORRECT;
            Units = Enum.GetValues(typeof(MarbleUnit)) as MarbleUnit[];
            //OrderBy = Enum.GetValues(typeof(OrderOption)) as OrderOption[];
            Keyword = keyword;

            ScrollKinds = Enum.GetValues(typeof(ScrollKind)) as ScrollKind[];

            TabKind = kind;

            #region Action

            #region _clearCommand = new Command(...)

            _clearCommand = new Command(param =>
            {
                Clear();
                //ActionBoardVisibility = Visibility.Collapsed;
            });

            #endregion _clearCommand = new Command(...)

            #region _globalTimeCommand = new Command(...)

            _globalTimeCommand = new Command(param =>
            {
                SetPositioningStrategy(MarblePositioningStrategy.GlobalTime);
                //ActionBoardVisibility = Visibility.Collapsed;
            });

            #endregion _globalTimeCommand = new Command(...)

            #region _privateTimeCommand = new Command(...)

            _privateTimeCommand = new Command(param =>
            {
                SetPositioningStrategy(MarblePositioningStrategy.PrivateTime);
                //ActionBoardVisibility = Visibility.Collapsed;
            });

            #endregion _privateTimeCommand = new Command(...)

            #region _sequenceCommand = new Command(...)

            _sequenceCommand = new Command(param =>
            {
                SetPositioningStrategy(MarblePositioningStrategy.Sequence);
                //ActionBoardVisibility = Visibility.Collapsed;
            });

            #endregion _sequenceCommand = new Command(...)

            #region _showAction = new Command(...)

            _showAction = new Command(param =>
            {
                if (ActionBoardVisibility == Visibility.Visible)
                    ActionBoardVisibility = Visibility.Collapsed;
                else
                    ActionBoardVisibility = Visibility.Visible;
            });

            #endregion _showAction = new Command(...)

            #region _scrollAction = new Command(...)

            _scrollAction = new Command(param =>
            {
                IsScrollToEnd = !IsScrollToEnd;
            });

            #endregion _scrollAction = new Command(...)

            #endregion Action
        }

        #endregion Constructors

        #region Properties

        #region TabKind

        /// <summary>
        /// Gets the kind of the tab.
        /// </summary>
        /// <value>
        /// The kind of the tab.
        /// </value>
        public TabKind TabKind { get; private set; }

        #endregion TabKind

        #region FlatItems

        /// <summary>
        /// Gets or sets the flat items.
        /// </summary>
        /// <value>
        /// The flat items.
        /// </value>
        public MarbleCollection<MarbleBase> FlatItems { get; set; }

        #endregion FlatItems

        #region CanvasLeft

        /// <summary>
        /// Gets or sets the canvas left.
        /// </summary>
        /// <value>
        /// The canvas left.
        /// </value>
        public double CanvasLeft
        {
            get { return _canvasLeft; }
            set
            {
                _canvasLeft = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CanvasLeft"));
            }
        }

        #endregion CanvasLeft

        #region ScrollText

        private string _scrollText = SCROLL_TO_END;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is scroll to end.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is scroll to end; otherwise, <c>false</c>.
        /// </value>
        public string ScrollText
        {
            get { return _scrollText; }
            private set
            {
                _scrollText = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ScrollText"));
            }
        }

        #endregion ScrollText

        #region LastScroll

        /// <summary>
        /// Gets or sets the last scroll.
        /// used for the last scrolling gap optimization at the ScrollViewerExtensions
        /// </summary>
        /// <value>
        /// The last scroll.
        /// </value>
        public double LastScroll { get; set; }

        #endregion LastScroll

        #region IsScrollToEnd

        /// <summary>
        /// Gets or sets a value indicating whether this instance is scroll to end.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is scroll to end; otherwise, <c>false</c>.
        /// </value>
        public bool IsScrollToEnd
        {
            get { return _isScrollToEnd; }
            set
            {
                _isScrollToEnd = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsScrollToEnd"));
                if (IsScrollToEnd)
                    ScrollText = SCROLL_DEFAULT;
                else
                    ScrollText = SCROLL_TO_END;
            }
        }

        #endregion IsScrollToEnd

        #region ScrollKindSelected

        /// <summary>
        /// Gets or sets the scroll kind selected.
        /// </summary>
        /// <value>
        /// The scroll kind selected.
        /// </value>
        public ScrollKind ScrollKindSelected
        {
            get { return _scrollKindSelected; }
            set
            {
                _scrollKindSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ScrollKindSelected"));
                IsScrollToEnd = value == ScrollKind.ScrollToEnd;
            }
        }

        #endregion ScrollKindSelected

        #region ScrollKinds

        /// <summary>
        /// Gets or sets the scroll kinds.
        /// </summary>
        /// <value>
        /// The scroll kinds.
        /// </value>
        public ScrollKind[] ScrollKinds { get; set; }

        #endregion ScrollKinds

        #region Keyword

        /// <summary>
        /// Gets the keyword.
        /// </summary>
        public string Keyword { get; private set; }

        #endregion Keyword

        #region Index

        /// <summary>
        /// Gets the index.
        /// </summary>
        public double Index { get; private set; }

        #endregion Index

        #region UnitSelected

        /// <summary>
        /// Gets or sets the unit selected.
        /// </summary>
        /// <value>
        /// The unit selected.
        /// </value>
        public MarbleUnit UnitSelected
        {
            get { return _unitSelected; }
            set
            {
                _canvasLeft = 0;
                _unitSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs("UnitSelected"));
                Diagrams.View.Refresh();
            }
        }

        #endregion UnitSelected

        #region Units

        /// <summary>
        /// Gets or sets the units of timeline.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public MarbleUnit[] Units { get; set; }

        #endregion Units

        #region Diagrams

        /// <summary>
        /// Gets the diagrams.
        /// </summary>
        public MarbleCollection<MarbleDiagramModel> Diagrams { get; private set; }

        #endregion Diagrams

        #region DiagramWidth

        /// <summary>
        /// Gets the width of the diagram.
        /// </summary>
        /// <value>
        /// The width of the diagram.
        /// </value>
        public double DiagramWidth
        {
            get { return _diagramWidth; }
            private set
            {
                _diagramWidth = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DiagramWidth"));
            }
        }

        #endregion DiagramWidth

        #region DiagramScale

        /// <summary>
        /// Gets or sets the diagram scale.
        /// </summary>
        /// <value>
        /// The diagram scale.
        /// </value>
        public double DiagramScale
        {
            get { return _diagramScale; }
            set
            {
                _diagramScale = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DiagramScale"));
                PropertyChanged(this, new PropertyChangedEventArgs("DiagramWidth"));
                PropertyChanged(this, new PropertyChangedEventArgs("TextScale"));
            }
        }

        #endregion DiagramScale

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

        #region ViewModel

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public IViewModel ViewModel => _vm;

        #endregion // ViewModel

        #endregion Properties

        #region Methods

        #region AppendMarble

        /// <summary>
        /// Appends the marble.
        /// </summary>
        /// <param name="itemWrapper">The item wrapper.</param>
        public void AppendMarble(MarbleItemViewModel itemWrapper)
        {
            #region Validation

            if (itemWrapper == null)
                throw new NullReferenceException("itemWrapper");

            #endregion Validation

            MarbleBase item = itemWrapper.Item;

            if (TabKind == TabKind.Flat)
            {
                FlatItems.AddAsync(item);
                return;
            }

            itemWrapper.MainContext = this;

            #region Diagrams.AddAsync(diagram)

            MarbleDiagramModel diagram;
            if (!_marbleDiagrams.TryGetValue(item.Name, out diagram))
            {
                diagram = new MarbleDiagramModel(item.Name, item.IndexOrder, this);
                if (_marbleDiagrams.TryAdd(item.Name, diagram))
                    Diagrams.AddAsync(diagram);
                else
                {
                    diagram.Dispose();
                    diagram = _marbleDiagrams[item.Name];
                }
            }

            #endregion // Diagrams.AddAsync(diagram)

            itemWrapper = itemWrapper.Clone(diagram);
            diagram.AppendMarble(itemWrapper);
        }

        #endregion AppendMarble

        #region SetDiagramOrdering

        /// <summary>
        /// Sets the diagram ordering.
        /// </summary>
        private void SetDiagramOrdering()
        {
            Diagrams.View.SortDescriptions.Clear();
            Diagrams.View.SortDescriptions.Add(new SortDescription("IndexOrder", ListSortDirection.Ascending));
            Diagrams.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        #endregion // SetDiagramOrdering

        #region TranslateOffset

        /// <summary>
        /// Translates the offset.
        /// </summary>
        /// <param name="itemWrapper">The item wrapper.</param>
        /// <returns></returns>
        public double TranslateOffset(MarbleItemViewModel itemWrapper)
        {
            #region Validation

            if (itemWrapper == null)
                throw new NullReferenceException("itemWrapper");

            #endregion Validation

            MarbleBase item = itemWrapper.Item;
            TimeSpan offset = item.Offset;
            IMarbleDiagramContext diagram = itemWrapper.DiagramContext;

            double totalSeconds = offset.TotalSeconds;

            double initTimeInSeconds = _initTimeInSeconds;
            double totalSecondsByDiagram;
            if (_initTimeInSeconds == -1)
                initTimeInSeconds = Interlocked.CompareExchange(ref _initTimeInSeconds, totalSeconds, -1);
            totalSecondsByDiagram = diagram.GetLocalTimeInSeconds(totalSeconds);

            #region totalSeconds = ... (PositioningStrategy)

            switch (diagram.PositioningStrategy)
            {
                case MarblePositioningStrategy.GlobalTime:
                    totalSeconds -= _initTimeInSeconds;
                    totalSeconds = TranslateByFactor(offset, totalSeconds);
                    break;

                case MarblePositioningStrategy.PrivateTime:
                    totalSeconds = TranslateByFactor(offset, totalSecondsByDiagram);
                    break;

                case MarblePositioningStrategy.Sequence:
                    totalSeconds = itemWrapper.Sequence;
                    break;
            }

            #endregion totalSeconds = ... (PositioningStrategy)

            int correction = diagram.Size + MarbleDiagramModel.DIAGRAM_WIDTH_CORRECTION;
            double result = totalSeconds * (itemWrapper.DiagramContext.Size * 2 + 5);
            if ((item.Kind == MarbleKind.OnCompleted || item.Kind == MarbleKind.OnError) &&
                diagram.PositioningStrategy != MarblePositioningStrategy.Sequence)
            {
                result += itemWrapper.DiagramContext.Size;
            }

            double candidateWidth = result + correction;
            if (candidateWidth > _diagramWidth)
            {
                _diagramWidth = candidateWidth;
                PropertyChanged(this, new PropertyChangedEventArgs("DiagramWidth"));
            }

            return result;
        }

        #endregion TranslateOffset

        #region TranslateByFactor

        /// <summary>
        /// Translates the by factor.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="totalSeconds">The total seconds.</param>
        /// <returns></returns>
        private double TranslateByFactor(TimeSpan offset, double totalSeconds)
        {
            switch (_unitSelected)
            {
                case MarbleUnit.Seconds0001:
                    totalSeconds = offset.TotalMilliseconds;
                    break;

                case MarbleUnit.Seconds001:
                    totalSeconds = offset.TotalMilliseconds / 10;
                    break;

                case MarbleUnit.Seconds01:
                    totalSeconds = offset.TotalMilliseconds / 100;
                    break;

                case MarbleUnit.Seconds:
                    break;

                case MarbleUnit.Minute:
                    totalSeconds = offset.TotalMinutes;
                    break;

                case MarbleUnit.Hour:
                    totalSeconds = offset.TotalHours;
                    break;

                case MarbleUnit.Day:
                    totalSeconds = offset.TotalDays;
                    break;
                //case MarbleUnit.Sequential:
                //    value = item.Sequesnce;
                //    break;
            }

            return totalSeconds;
        }

        #endregion TranslateByFactor

        #region Clear

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Interlocked.Exchange(ref _initTimeInSeconds, -1);
            _marbleDiagrams.Clear();
            Diagrams.Clear();
            FlatItems.Clear();
            DiagramWidth = RESET_WIDTH;
        }

        #endregion Clear

        #region SetViewModel

        /// <summary>
        /// Sets the ViewModel.
        /// </summary>
        /// <param name="vm">The view-model.</param>
        void ITabModel.SetViewModel(IViewModel vm)
        {
            if (_vm != null)
                throw new ArgumentException("cannot set the ViewModel because it is already sets.");
            _vm = vm;
            _diagramWidth = _vm.Width - WIN_WIDTH_CORRECT;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(ViewModel)));
        }

        #endregion SetViewModel

        #region SetPositioningStrategy

        /// <summary>
        /// Sets the positioning strategy.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        public void SetPositioningStrategy(MarblePositioningStrategy strategy)
        {
            foreach (var diagram in Diagrams)
            {
                diagram.PositioningStrategy = strategy;
            }
        }

        #endregion SetPositioningStrategy

        #endregion Methods

        #region Commands

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

        #region ShowAction

        /// <summary>
        /// Show actions.
        /// </summary>
        public ICommand ShowAction
        {
            get
            {
                return _showAction;
            }
        }

        #endregion ShowAction

        #region ShowAction

        /// <summary>
        /// Show actions.
        /// </summary>
        public ICommand ScrollAction
        {
            get
            {
                return _scrollAction;
            }
        }

        public MarbleDiagramsViewModel MarbleController { get; private set; }

        #endregion ShowAction

        #endregion Commands
    }
}