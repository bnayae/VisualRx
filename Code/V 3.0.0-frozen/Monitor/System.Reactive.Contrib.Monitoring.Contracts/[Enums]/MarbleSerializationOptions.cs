#region Using

using System.Runtime.Serialization;


#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// Marble raw value serialization options
    /// </summary>
    [DataContract]
    [Flags]
    public enum MarbleSerializationOptions
    {
        /// <summary>
        /// raw value none serialized
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// raw value serialized using Serializable
        /// </summary>
        [EnumMember]
        Serializable = 1,

        /// <summary>
        /// raw value serialized using NetDataContractSerialization
        /// </summary>
        [EnumMember]
        NetDataContractSerialization = 2,

        /// <summary>
        /// take the ToString representation
        /// </summary>
        [EnumMember]
        ToString = 4,
    }
}