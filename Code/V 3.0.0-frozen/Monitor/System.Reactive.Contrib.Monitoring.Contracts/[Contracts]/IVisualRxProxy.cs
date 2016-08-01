using System.Collections.Generic;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// the contract of Visual Rx proxy
    /// (proxy is responsible to send the monitored datum through a channel)
    /// </summary>
    public interface IVisualRxProxy: IDisposable
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>initialize information</returns>
        string OnInitialize();

        /// <summary>
        /// Send a balk.
        /// </summary>
        /// <param name="items">The items.</param>
        void OnBulkSend(IEnumerable<MarbleBase> items);

        /// <summary>
        /// Gets the monitor provider kind.
        /// </summary>
        string Kind { get; }

        SendThreshold SendingThreshold { get; }
    }
}