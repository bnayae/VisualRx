#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Windows.Media;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    // TODO: enable plugin to set Height / Color / Background

    /// <summary>
    /// UI side wrapper to the marble item
    /// </summary>
    [DebuggerDisplay("{Item.Name}, Value = {Item.Value}, Left = {Left}")]
    public class MarbleItemViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleItemViewModel"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public MarbleItemViewModel(MarbleBase item)
        {
            Item = item;
        }

        #endregion Constructors

        private IMainContext _mainContext;

        #region Item

        /// <summary>
        /// Gets the item.
        /// </summary>
        public MarbleBase Item { get; private set; }

        #endregion Item

        #region DiagramContext

        /// <summary>
        /// Gets or sets the diagram context.
        /// </summary>
        /// <value>
        /// The diagram context.
        /// </value>
        public IMarbleDiagramContext DiagramContext { get; set; }

        #endregion DiagramContext

        #region MainContext

        /// <summary>
        /// UI context of execution
        /// </summary>
        public IMainContext MainContext
        {
            get { return _mainContext; }
            set
            {
                _mainContext = value;
            }
        }

        #endregion MainContext

        #region Left

        /// <summary>
        /// Gets the left.
        /// </summary>
        public double Left
        {
            get
            {
                var offset = MainContext.TranslateOffset(this);
                return offset;
            }
        }

        #endregion Left

        #region Sequence

        /// <summary>
        /// Gets or sets the item sequence (within a marble diagram).
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public int Sequence { get; set; }

        #endregion Sequence

        #region Color

        /// <summary>
        /// Gets or sets the color of the item.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public SolidColorBrush Color { get; set; }

        #endregion Color

        #region Clone

        /// <summary>
        /// Clones the specified diagram.
        /// </summary>
        /// <param name="diagram">The diagram.</param>
        /// <returns></returns>
        public MarbleItemViewModel Clone(IMarbleDiagramContext diagram)
        {
            var marble = new MarbleItemViewModel(Item);
            var item = marble.Item;
            marble.MainContext = diagram.MainContext;
            marble.DiagramContext = diagram;
            marble.Color = diagram.Color;
            return marble;
        }

        #endregion Clone
    }
}