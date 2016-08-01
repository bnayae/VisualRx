using System.Collections.Generic;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// the contract of Visual Rx proxy
    /// (proxy is responsible to send the monitored datum through a channel)
    /// </summary>
    public interface IVisualRxFilterableProxy: IVisualRxProxy 
    {
        /// <summary>
        /// Filters the specified item.
        /// </summary>
        /// <param name="candidate">The value and metadata</param>
        /// <returns></returns>
        bool Filter(MarbleCandidate candidate);
    }
}