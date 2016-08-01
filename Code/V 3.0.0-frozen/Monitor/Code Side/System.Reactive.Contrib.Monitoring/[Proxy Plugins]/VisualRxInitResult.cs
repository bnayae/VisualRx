#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Text;
using System.Threading;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Load information on the monitor proxy plug-ins
    /// </summary>
    public partial class VisualRxInitResult : IEnumerable<VisualRxInitResult.VisualRxProxyInfo>
    {
        private readonly ConcurrentQueue<VisualRxProxyInfo> _proxiesInfo = new ConcurrentQueue<VisualRxProxyInfo>();

        #region Add

        /// <summary>
        /// Adds the specified proxy.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        internal VisualRxProxyInfo Add(VisualRxProxyWrapper proxy)
        {
            VisualRxProxyInfo info = new VisualRxProxyInfo(proxy.Kind);
            _proxiesInfo.Enqueue(info);
            return info;
        }

        #endregion Add

        #region Count

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count { get { return _proxiesInfo.Count; } }

        #endregion Count

        #region ToString

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder(_proxiesInfo.Count * 100);
            sb.AppendLine("Loaded Proxies:");
            foreach (var item in _proxiesInfo) // blocking
            {
                sb.AppendFormat("\t{0}, loaded = {1}\r\n", item.Kind, item.Succeed);
                if (!string.IsNullOrWhiteSpace(item.InitInfo))
                    sb.AppendFormat("\t{0}\r\n", item.InitInfo.Replace("\n", "\n\t\t"));
                if (item.Error != null)
                    sb.AppendFormat("\t{0}\r\n", item.Error);
            }

            return sb.ToString();
        }

        #endregion ToString

        #region IEnumerator<VisualRxProxyInfo> Members

        #region Overloads

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Overloads

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<VisualRxProxyInfo> GetEnumerator()
        {
            return _proxiesInfo.GetEnumerator();
        }

        #endregion IEnumerator<VisualRxProxyInfo> Members
    }
}