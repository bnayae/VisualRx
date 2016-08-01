using System;
using System.Reactive.Contrib.Monitoring.Contracts;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    public interface IMonitorSurrogate<in T>
    {
        /// <summary>
        /// Map a type into a surrogate type
        /// </summary>
        /// <param name="item"></param>
        /// <param name="candidate">value + metadata</param>
        /// <returns></returns>
        object Mapping(T item, MarbleCandidate candidate);

        /// <summary>
        /// set the serialization strategy.
        /// </summary>
        /// <value>
        /// The serialization strategy.
        /// </value>
        MarbleSerializationOptions SerializationStrategy { get; }
    }
}
