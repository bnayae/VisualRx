#region Using



#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// common constants
    /// </summary>
    public abstract class Constants
    {
        /// <summary>
        /// the namespace of the Visual Rx service
        /// </summary>
        public const string ServiceNamespace = "urn:System.Reactive.Contrib.Monitoring.Contracts.Service";

        /// <summary>
        /// the namespace of the Visual Rx data contract
        /// </summary>
        public const string DataNamespace = "urn:RxContrib";

        /// <summary>
        /// the default trace name
        /// </summary>
        public const string TraceName = "System.Reactive.Contrib.VisualRx";
    }
}