using System.Runtime.Serialization;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// marble kind
    /// </summary>
    [DataContract]
    public enum MarbleKind
    {
        /// <summary>
        /// On next
        /// </summary>
        [EnumMember]
        OnNext = 0,

        /// <summary>
        /// On error
        /// </summary>
        [EnumMember]
        OnError = 1,

        /// <summary>
        /// On complete
        /// </summary>
        [EnumMember]
        OnCompleted = 2,
    }
}