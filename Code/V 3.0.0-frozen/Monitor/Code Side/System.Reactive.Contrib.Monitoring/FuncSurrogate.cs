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
    /// flexible implementation of the surrogate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FuncSurrogate<T> : IMonitorSurrogate<T>
    {
        private readonly Func<T, MarbleCandidate, object> _surrogate;
        private readonly MarbleSerializationOptions _serializationStrategy;
        public FuncSurrogate(Func<T, MarbleCandidate, object> surrogate, 
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString)
        {
            _surrogate = surrogate;
            _serializationStrategy = serializationStrategy;
        }

        #region SerializationStrategy

        /// <summary>
        /// Gets the serialization strategy.
        /// </summary>
        /// <value>
        /// The serialization strategy.
        /// </value>
        public MarbleSerializationOptions SerializationStrategy
        {
            get { return _serializationStrategy; }
        }

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
            return _surrogate(item, candidate); ;
        }

        #endregion // Mapping
    }
}