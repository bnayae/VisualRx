using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.ServiceModel;
using System.Text;

namespace System.Reactive.Contrib.Monitoring.UI
{
    /// <summary>
    /// Monitor queued service
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single, 
        UseSynchronizationContext=false)]
    public class MonitorServiceAdapter : IVisualRxServiceAdapter
    {
        #region Send

        /// <summary>
        /// Sends the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void Send(MarbleBase[] items)
        {
            MarbleController.AddMarbleItem(items);
        }

        #endregion Send

        #region Ping

        public void Ping()
        {
            MarbleController.ViewModel.StartAnimation();
        }

        #endregion Ping
    }
}