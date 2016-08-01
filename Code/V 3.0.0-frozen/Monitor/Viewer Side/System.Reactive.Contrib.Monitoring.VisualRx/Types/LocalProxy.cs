using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.UI;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Send monitor info through WCF channel
    /// </summary>
    public class LocalProxy : VisualRxProxyBase, IVisualRxFilterableProxy
    {
        #region Properties

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind => "Local"; 

        #endregion Kind

        #endregion Properties

        #region Methods

        #region OnBulkSend

        /// <summary>
        /// Send a balk.
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            MarbleController.AddMarbleItem(items.ToArray());
        }

        #endregion OnBulkSend

        #region OnInitialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public string OnInitialize() => "Local";

        #endregion OnInitialize

        #endregion Methods

        #region Create

        public static LocalProxy Create()
        {
            return new LocalProxy();
        }

        #endregion // Create

        #region Dispose

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposed)
        {
        }

        #endregion // Dispose
    }
}
