#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;

#endregion Using

namespace System.Reactive.Linq
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class VisualRxExtensions
    {
        #region Monitor Many

        #region Overloads

        /// <summary>
        /// Monitors many streams (like window).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IObservable<T>> MonitorMany<T>(
            this IObservable<IObservable<T>> instance,
            string name,
            double orderingBaseIndex,
            params string[] keywords)
        {
            return MonitorMany(instance, name, orderingBaseIndex, null as IMonitorSurrogate<T>, keywords);
        }

        /// <summary>
        /// Monitors many streams (like window).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="surrogateAction">The surrogate action.</param>
        /// <param name="serializationStrategy">The serialization strategy.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IObservable<T>> MonitorMany<T>(
            this IObservable<IObservable<T>> instance,
            string name,
            double orderingBaseIndex,
            Func<T, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<T> surrogate = new FuncSurrogate<T>(surrogateAction, serializationStrategy);
            return MonitorMany(instance, name, orderingBaseIndex, surrogate, keywords);
        }

        #endregion // Overloads

        /// <summary>
        /// Monitors many streams (like window).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="surrogate">The surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IObservable<T>> MonitorMany<T>(
            this IObservable<IObservable<T>> instance,
            string name,
            double orderingBaseIndex,
            IMonitorSurrogate<T> surrogate,
            params string[] keywords)
        {
            int index = 0;
            var xs = from obs in instance
                     let idx = Interlocked.Increment(ref index)
                     select obs.Monitor(name + " " + idx, orderingBaseIndex + (idx / 100000.0), surrogate, keywords);
            return xs;
        }

        #endregion Monitor Many

        #region Monitor Group

        #region Overloads

        /// <summary>
        /// Monitor Group by stream
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IGroupedObservable<TKey, TElement>> MonitorGroup<TKey, TElement>(
            this IObservable<IGroupedObservable<TKey, TElement>> instance,
            string name,
            double orderingBaseIndex,
            params string[] keywords)
        {
            return MonitorGroup(instance, name, orderingBaseIndex, null as IMonitorSurrogate<TElement>, keywords);
        }

        /// <summary>
        /// Monitor Group by stream
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="surrogateAction">The surrogate action.</param>
        /// <param name="serializationStrategy">The serialization strategy.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IGroupedObservable<TKey, TElement>> MonitorGroup<TKey, TElement>(
            this IObservable<IGroupedObservable<TKey, TElement>> instance,
            string name,
            double orderingBaseIndex,
            Func<TElement, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<TElement> surrogate = new FuncSurrogate<TElement>(surrogateAction, serializationStrategy);
            return MonitorGroup(instance, name, orderingBaseIndex, surrogate, keywords);
        }

        #endregion // Overloads

        /// <summary>
        /// Monitor Group by stream
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingBaseIndex">Index of the ordering base.</param>
        /// <param name="elementSurrogate">The element surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<IGroupedObservable<TKey, TElement>> MonitorGroup<TKey, TElement>(
            this IObservable<IGroupedObservable<TKey, TElement>> instance,
            string name,
            double orderingBaseIndex,
            IMonitorSurrogate<TElement> elementSurrogate,
            params string[] keywords)
        {
            var keySurrogate = new MonitorSurrogate<IGroupedObservable<TKey, TElement>>(
                (g, m) => "Key = " + g.Key);

            instance = instance.Monitor(name + " (keys)", orderingBaseIndex,
                keySurrogate, keywords);

            int index = 0;
            var xs = from g in instance
                     let idx = Interlocked.Increment(ref index)
                     let ord = orderingBaseIndex + (idx / 100000.0)
                     select new GroupedMonitored<TKey, TElement>(
                                        g, $"{name}:{g.Key} ({idx})", ord, elementSurrogate, keywords);

            return xs;
        }


        #endregion Monitor Group

        #region Monitor IConnectableObservable

        #region Overloads

        /// <summary>
        /// Monitor IConnectableObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IConnectableObservable<T> Monitor<T>(
            this IConnectableObservable<T> instance,
            string name,
            double orderingIndex,
            params string[] keywords)
        {
            return Monitor<T>(instance, name, orderingIndex, null as IMonitorSurrogate<T>, keywords);
        }

        /// <summary>
        /// Monitor IConnectableObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogateAction">The surrogate action.</param>
        /// <param name="serializationStrategy">The serialization strategy.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IConnectableObservable<T> Monitor<T>(
            this IConnectableObservable<T> instance,
            string name,
            double orderingIndex,
            Func<T, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<T> surrogate = new FuncSurrogate<T>(surrogateAction, serializationStrategy);
            return Monitor<T>(instance, name, orderingIndex, surrogate, keywords);
        }
        
        #endregion Overloads

        /// <summary>
        /// Monitor IConnectableObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogate">The surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IConnectableObservable<T> Monitor<T>(
            this IConnectableObservable<T> instance,
            string name,
            double orderingIndex, 
            IMonitorSurrogate<T> surrogate,
            params string[] keywords)
        {
            var monitor = new MonitorOperator<T>(
                name, orderingIndex, surrogate, keywords);

            var watcher = monitor.AttachTo(instance);
            return watcher;
        }

        #endregion Monitor IConnectableObservable

        #region Monitor ISubject

        #region Overloads

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<T> Monitor<T>(
            this ISubject<T> instance,
            string name,
            double orderingIndex,
            params string[] keywords)
        {
            return Monitor<T>(
                instance, 
                name, 
                orderingIndex,
                null as IMonitorSurrogate<T>, keywords);
        }

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogateAction">The surrogate action.</param>
        /// <param name="serializationStrategy">The serialization strategy.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<T> Monitor<T>(
            this ISubject<T> instance,
            string name,
            double orderingIndex,
            Func<T, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<T> surrogate = new FuncSurrogate<T>(surrogateAction, serializationStrategy);
            return Monitor<T>(
                instance,
                name,
                orderingIndex,
                surrogate, keywords);
        }

        #endregion Overloads

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogate">The surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<T> Monitor<T>(
            this ISubject<T> instance,
            string name,
            double orderingIndex,
            IMonitorSurrogate<T> surrogate,
            params string[] keywords)
        {
            var monitor = new MonitorOperator<T>(
                name, orderingIndex, surrogate, keywords);

            var watcher = monitor.AttachTo(instance);
            return watcher;

        }

        #region Overloads

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<TIn, TOut> Monitor<TIn, TOut>(
            this ISubject<TIn, TOut> instance,
            string name,
            double orderingIndex,
            params string[] keywords)
        {
            return Monitor<TIn, TOut>(instance, name, orderingIndex, null as IMonitorSurrogate<TOut>, keywords);
        }

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogateAction">The surrogate action.</param>
        /// <param name="serializationStrategy">The serialization strategy.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<TIn, TOut> Monitor<TIn, TOut>(
            this ISubject<TIn, TOut> instance,
            string name,
            double orderingIndex,
            Func<TOut, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<TOut> surrogate = new FuncSurrogate<TOut>(surrogateAction, serializationStrategy);
            return Monitor<TIn, TOut>(instance, name, orderingIndex, surrogate, keywords);
        }

        #endregion Overloads

        /// <summary>
        /// Monitor ISubject stream
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogate">The surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static ISubject<TIn, TOut> Monitor<TIn, TOut>(
            this ISubject<TIn, TOut> instance,
            string name,
            double orderingIndex,
            IMonitorSurrogate<TOut> surrogate,
            params string[] keywords)
        {
            var monitor = new MonitorOperator<TOut>(
                name, orderingIndex, surrogate, keywords);

            var watcher = monitor.AttachTo(instance);
            return watcher;
        }

        #endregion Monitor ISubject

        #region Monitor

        #region Overloads

        /// <summary>
        /// Monitor IObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">the ordering of the marble diagram</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<T> Monitor<T>(
            this IObservable<T> instance,
            string name,
            double orderingIndex,
            params string[] keywords)
        {
            return Monitor<T>(
                instance, name, orderingIndex, 
                null as IMonitorSurrogate<T> /* convertToValue */,
                keywords);
        }

        /// <summary>
        /// Monitor IObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogateAction">a surrogate action.</param>
        /// <param name="serializationStrategy">serialization instruction</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<T> Monitor<T>(
            this IObservable<T> instance,
            string name,
            double orderingIndex,
            Func<T, MarbleCandidate, object> surrogateAction,
            MarbleSerializationOptions serializationStrategy = MarbleSerializationOptions.ToString,
            params string[] keywords)
        {
            IMonitorSurrogate<T> surrogate = new FuncSurrogate<T>(surrogateAction, serializationStrategy);
            return Monitor<T>(
                instance, name, orderingIndex,
                surrogate,
                keywords);
        }

        #endregion Overloads

        /// <summary>
        /// Monitor IObservable stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="orderingIndex">Index of the ordering.</param>
        /// <param name="surrogate">a surrogate.</param>
        /// <param name="keywords">The keywords.</param>
        /// <returns></returns>
        public static IObservable<T> Monitor<T>(
            this IObservable<T> instance,
            string name,
            double orderingIndex,
            IMonitorSurrogate<T> surrogate,
            params string[] keywords)
        {
            var monitor = new MonitorOperator<T>(
                name, orderingIndex, surrogate, keywords);

            var watcher = monitor.AttachTo(instance);
            return watcher;
        }

        #endregion Monitor

        #region GroupedMonitored (nested class)

        /// <summary>
        /// Group wrapper
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        private class GroupedMonitored<TKey, TElement> : IGroupedObservable<TKey, TElement>
        {
            private readonly TKey _key;
            private readonly IObservable<TElement> _groupStream;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="GroupedMonitored{TKey, TElement}" /> class.
            /// </summary>
            /// <param name="group">The group.</param>
            /// <param name="name">The name.</param>
            /// <param name="order">The order.</param>
            /// <param name="surrogate">The surrogate.</param>
            /// <param name="keywords">The keywords.</param>
            public GroupedMonitored(
                IGroupedObservable<TKey, TElement> group,
                string name,
                double order,
                IMonitorSurrogate<TElement> surrogate,
                params string[] keywords)
            {
                _key = group.Key;
                _groupStream = group.Monitor(name, order, surrogate, keywords);
            }

            #endregion // Ctor

            #region Key

            /// <summary>
            /// Gets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            /// <exception cref="System.NotImplementedException"></exception>
            public TKey Key => _key;

            #endregion // Key

            #region Subscribe

            /// <summary>
            /// Subscribes the specified observer.
            /// </summary>
            /// <param name="observer">The observer.</param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<TElement> observer)
            {
                return _groupStream.Subscribe(observer);
            }

            #endregion // Subscribe
        }

        #endregion // GroupedMonitored (nested class)
    }
}