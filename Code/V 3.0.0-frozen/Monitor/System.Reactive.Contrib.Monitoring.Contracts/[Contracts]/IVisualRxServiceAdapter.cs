#region Using

using System.ServiceModel;


#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// the contract of the Visual RX cross process communication
    /// </summary>
    [ServiceContract(Namespace = Constants.ServiceNamespace, Name = "IVisualRxServiceAdapter")]
    [DeliveryRequirements(QueuedDeliveryRequirements = QueuedDeliveryRequirementsMode.Allowed)]
    public interface IVisualRxServiceAdapter
    {
        /// <summary>
        /// Sends the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        [OperationContract(IsOneWay = true)]
        void Send(MarbleBase[] items);

        /// <summary>
        /// Pings this instance.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Ping();
    }
}