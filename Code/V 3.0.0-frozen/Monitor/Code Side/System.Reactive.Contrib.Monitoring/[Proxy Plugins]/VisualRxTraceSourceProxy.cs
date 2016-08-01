#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;

#endregion Using

// TODO: LIMIT the QueueSubject buffer size by config

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Trace source monitor's proxy
    /// </summary>
    public sealed class VisualRxTraceSourceProxy : VisualRxProxyBase, IVisualRxFilterableProxy
    {
        #region Constants

        private const string ARRAY_NS = " xmlns:a=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"";
        private const string MARBLE_NS = " xmlns=\"urn:RxContrib\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\"";

        /// <summary>
        /// the VisualRxTraceSourceProxy kind
        /// </summary>
        public const string KIND = "TraceSource";

        private const string TRACE_NAME = "VisualRx";
        private const int MAX_ERRORS = 20;
        private static readonly string NAME = typeof(VisualRxTraceSourceProxy).Name;

        #endregion Constants

        #region Private / Protected Fields

        private static readonly TraceSource _trace = new TraceSource(TRACE_NAME);
        private readonly DataContractSerializer _serializer = new DataContractSerializer(typeof(MarbleBase));
        private int _traceId = 0;
        private bool _prevError = false;
        private int _sequentialErrCount = 0;

        #endregion Private / Protected Fields

        #region Kind

        /// <summary>
        /// Gets the proxy kind.
        /// </summary>
        public string Kind { get { return KIND; } }

        #endregion Kind

        #region ParseXml

        /// <summary>
        /// Parses the XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static string ParseXml(Stream xml)
        {
            TextWriter textWriter = new StringWriter();
            using (XmlReader xmlReader = new XmlTextReader(xml))
            {
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "  ",
                    NewLineOnAttributes = false,
                    NewLineHandling = NewLineHandling.None,
                    NamespaceHandling = NamespaceHandling.OmitDuplicates,
                };
                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    while (xmlReader.Read())
                        xmlWriter.WriteNode(xmlReader, false);
                }
                return textWriter.ToString();
            }
        }

        #endregion ParseXml

        #region OnBulkSend

        /// <summary>
        /// Bulk send.
        /// </summary>
        /// <param name="items">The items.</param>
        public void OnBulkSend(IEnumerable<MarbleBase> items)
        {
            foreach (var item in items)
            {
                try
                {
                    using (var srm = new MemoryStream())
                    {
                        _serializer.WriteObject(srm, item);
                        srm.Position = 0;
                        string text = ParseXml(srm)
                            .Replace(MARBLE_NS, string.Empty)
                            .Replace(ARRAY_NS, string.Empty);

                        _trace.TraceInformation("\r\n{0}\r\n", text);
                    }
                    _prevError = false;
                    Interlocked.Exchange(ref _sequentialErrCount, 0);
                }

                #region Exception Handling

                catch (Exception ex)
                {
                    var id = Interlocked.Increment(ref _traceId);
                    _prevError = true;
                    var sequentialErrCount = Interlocked.Increment(ref _sequentialErrCount);

                    #region Check for error threshhold

                    if (_prevError)
                    {
                        if (sequentialErrCount > MAX_ERRORS)
                        {
                            _trace.TraceData(TraceEventType.Error, id,
                                "Stop the VisualRxTraceSourceProxy because too many errors occurs" + ex.ToString());

                            return;
                        }
                    }

                    #endregion Check for error threshhold

                    _trace.TraceData(TraceEventType.Error, id,
                        "VisualRxTraceSourceProxy: Fail to log the item \r\n" + ex.ToString());
                }

                #endregion Exception Handling
            }
        }

        #endregion OnBulkSend

        #region OnInitialize

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns>
        /// initialize information
        /// </returns>
        public string OnInitialize()
        {
            return string.Empty; 
        }

        #endregion // OnInitialize

        #region Create

        /// <summary>
        /// Creates proxy.
        /// </summary>
        /// <returns></returns>
        public static VisualRxTraceSourceProxy Create()
        {
            return new VisualRxTraceSourceProxy();
        }

        #endregion Create

        #region Dispose

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [SecuritySafeCritical]
        protected override void Dispose(bool disposed)
        {
            try
            {
               _trace.Close();
            }
            catch (Exception ex)
            {
                TraceSourceMonitorHelper.Error("VisualRxTraceSourceProxy: {0}", ex);
            }
        }

        #endregion // Dispose
    }
}