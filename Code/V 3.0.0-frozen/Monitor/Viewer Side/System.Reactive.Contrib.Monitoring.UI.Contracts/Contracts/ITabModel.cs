#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Tab minimal contract
    /// </summary>
    public interface ITabModel
    {
        /// <summary>
        /// Gets the keyword.
        /// </summary>
        ///
        string Keyword { get; }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the ViewModel.
        /// </summary>
        /// <param name="vm">The view-model.</param>
        void SetViewModel(IViewModel vm);

        /// <summary>
        /// Appends a marble item.
        /// </summary>
        /// <param name="itemWrapper">The item wrapper.</param>
        void AppendMarble(MarbleItemViewModel itemWrapper);
    }
}