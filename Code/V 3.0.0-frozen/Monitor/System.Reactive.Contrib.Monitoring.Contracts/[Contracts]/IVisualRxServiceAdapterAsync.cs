#region Using

using System.ServiceModel;
using System.Threading.Tasks;


#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// the async contract of the Visual RX cross process communication
    /// </summary>
    [ServiceContract(Namespace = Constants.ServiceNamespace, Name = "IVisualRxServiceAdapter")]
    [DeliveryRequirements(QueuedDeliveryRequirements = QueuedDeliveryRequirementsMode.Allowed)]
    public interface IVisualRxServiceAdapterAsync : IVisualRxServiceAdapter
    {
        /// <summary>
        /// Sends the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        [OperationContract(Name="Send", IsOneWay = true)]
        Task SendAsync(MarbleBase[] items);

        /// <summary>
        /// Pings this instance.
        /// </summary>
        [OperationContract(Name = "Ping", IsOneWay = true)]
        Task PingAsync();
    }
}