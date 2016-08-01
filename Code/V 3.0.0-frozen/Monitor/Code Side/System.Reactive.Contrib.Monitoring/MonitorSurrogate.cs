#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Used to manipulate the sending item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonitorSurrogate<T> : IMonitorSurrogate<T>
    {
        private Func<T, MarbleCandidate, object> _mapping;

        #region Ctor

        public MonitorSurrogate(
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            Func<T, MarbleCandidate, object> mapping = null)
        {
            _mapping = mapping;
            SerializationStrategy = serializationStrategy;
        }

        public MonitorSurrogate(Func<T, MarbleCandidate, object> mapping) : 
            this(MarbleSerializationOptions.ToString, mapping)
        {
        }

        #endregion // Ctor

        #region SerializationStrategy

        /// <summary>
        /// Gets the serialization strategy.
        /// </summary>
        /// <value>
        /// The serialization strategy.
        /// </value>
        public MarbleSerializationOptions SerializationStrategy { get; private set; }

        #endregion // SerializationStrategy

        #region Mapping

        /// <summary>
        /// Mappings the specified candidate.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        public object Mapping(T item, MarbleCandidate candidate)
        {
            if (_mapping == null)
                return null;

            return _mapping(item, candidate);
        }

        #endregion // Mapping
    }
}