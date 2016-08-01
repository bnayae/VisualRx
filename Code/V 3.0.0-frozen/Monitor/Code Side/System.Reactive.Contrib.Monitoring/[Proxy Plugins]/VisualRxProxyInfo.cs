#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    partial class VisualRxInitResult
    {
        /// <summary>
        /// Load information on the monitor proxy plug-ins
        /// </summary>
        public class VisualRxProxyInfo
        {
            private Exception _error;

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualRxProxyInfo"/> class.
            /// </summary>
            /// <param name="kind">The kind.</param>
            public VisualRxProxyInfo(string kind)
            {
                Kind = kind;
                Succeed = true;
            }

            #endregion Ctor

            #region Name

            /// <summary>
            /// Gets the kind.
            /// </summary>
            public string Kind { get; private set; }

            #endregion Name

            #region Error

            /// <summary>
            /// Gets or sets the error.
            /// </summary>
            /// <value>
            /// The error.
            /// </value>
            public Exception Error
            {
                get { return _error; }
                internal set
                {
                    _error = value;
                    Succeed = false;
                }
            }

            #endregion Error

            #region InitInfo

            /// <summary>
            /// Gets or sets the init info.
            /// </summary>
            /// <value>
            /// The init info.
            /// </value>
            public string InitInfo { get; set; }

            #endregion InitInfo

            #region Succeed

            /// <summary>
            /// Gets or sets a value indicating whether the plugin loadeding succeed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
            /// </value>
            public bool Succeed { get; internal set; }

            #endregion Succeed
        }
    }
}