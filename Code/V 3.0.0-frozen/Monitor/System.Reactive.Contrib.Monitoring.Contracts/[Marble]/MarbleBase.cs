#region Using

using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// base class for the marble item
    /// </summary>
    [DataContract(Namespace = Constants.DataNamespace, IsReference = true)]
    [KnownType(typeof(MarbleError))]
    [KnownType(typeof(MarbleComplete))]
    [KnownType(typeof(MarbleNext))]
    [DebuggerDisplay("{Name}: {Kind}, {Value}, {Offset}")]
    public abstract class MarbleBase
    {
        #region Private / Protected Fields

        private static NetDataContractSerializer _ser = new NetDataContractSerializer();
        private static BinaryFormatter _formater = new BinaryFormatter();

        #endregion Private / Protected Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleBase"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        internal MarbleBase(
            string name,
            MarbleSerializationOptions options,
            MarbleKind kind,
            TimeSpan elapsed,
            string machineName)
        {
            Name = name;
            Kind = kind;
            ThreadId = Thread.CurrentThread.ManagedThreadId;

            Options = options;
            Offset = elapsed;

            DateCreatedUtc = DateTime.UtcNow;
            Keywords = new string[0];

            MachineName = machineName;
        }

        #endregion Constructors

        #region Properties

        #region Options

        /// <summary>
        /// Gets the options.
        /// </summary>
        [DataMember]
        public MarbleSerializationOptions Options { get; private set; }

        #endregion Options

        #region BinaryValue

        /// <summary>
        /// Gets or sets the binary value (only relevant at serialization time)
        /// </summary>
        /// <value>
        /// The binary value.
        /// </value>
        [DataMember]
        private byte[] BinaryValue { get; set; }

        #endregion BinaryValue

        #region StringValue

        /// <summary>
        /// Gets or sets the string value (only relevant at serialization time)
        /// </summary>
        /// <value>
        /// The string value.
        /// </value>
        [DataMember]
        private string StringValue { get; set; }

        #endregion StringValue

        #region RawValue

        /// <summary>
        /// marble raw value information
        /// </summary>
        public object RawValue { get; internal set; }

        #endregion RawValue

        #region Value

        /// <summary>
        /// marble value information
        /// </summary>
        public string Value
        {
            get
            {
                return FormattedValue ?? RawValue.ToString();
            }
        }

        #endregion Value

        #region FormattedValue

        /// <summary>
        /// marble formatted value information
        /// </summary>
        [DataMember]
        internal string FormattedValue
        {
            get;
            set;
        }

        #endregion FormattedValue

        #region ThreadId

        /// <summary>
        /// the thread id of the marble item
        /// </summary>
        [DataMember]
        public int ThreadId { get; private set; }

        #endregion ThreadId

        #region DateCreatedUtc

        /// <summary>
        /// Gets the date created.
        /// </summary>
        [DataMember]
        public DateTime DateCreatedUtc { get; private set; }

        #endregion DateCreatedUtc

        #region Name

        /// <summary>
        /// diagram name (sue as a key)
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        #endregion Name

        #region Kind

        /// <summary>
        /// type of the marble
        /// </summary>
        [DataMember]
        public MarbleKind Kind { get; private set; }

        #endregion Kind

        #region Offset

        /// <summary>
        /// Occurrence offset
        /// </summary>
        [DataMember]
        public TimeSpan Offset { get; private set; }

        #endregion Offset

        #region Keywords

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        [DataMember]
        public string[] Keywords { get; set; }

        #endregion Keywords

        #region IndexOrder

        /// <summary>
        /// the order index
        /// </summary>
        [DataMember]
        public double IndexOrder { get; set; }

        #endregion IndexOrder

        #region MachineName

        /// <summary>
        /// diagram name (sue as a key)
        /// </summary>
        [DataMember]
        public string MachineName { get; private set; }

        #endregion MachineName

        #endregion Properties

        #region Interception Points

        /// <summary>
        /// Called when [serializing].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if ((Options & MarbleSerializationOptions.Serializable) != MarbleSerializationOptions.None)
            {
                using (var srm = new MemoryStream())
                {
                    _formater.Serialize(srm, RawValue);
                    srm.Seek(0, SeekOrigin.Begin);
                    BinaryValue = srm.ToArray();
                }
            }
            else if ((Options & MarbleSerializationOptions.NetDataContractSerialization) != MarbleSerializationOptions.None)
            {
                using (var srm = new MemoryStream())
                {
                    _ser.Serialize(srm, RawValue);
                    srm.Seek(0, SeekOrigin.Begin);
                    BinaryValue = srm.ToArray();
                }
            }
            else
            {
                if (RawValue != null)
                    StringValue = RawValue.ToString();
            }
        }

        /// <summary>
        /// Called when [deserialized].
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if ((Options & MarbleSerializationOptions.Serializable) != MarbleSerializationOptions.None)
            {
                using (var srm = new MemoryStream(BinaryValue))
                {
                    RawValue = _formater.Deserialize(srm);
                }
                BinaryValue = null;
            }
            else if ((Options & MarbleSerializationOptions.NetDataContractSerialization) != MarbleSerializationOptions.None)
            {
                using (var srm = new MemoryStream(BinaryValue))
                {
                    RawValue = _ser.Deserialize(srm);
                }
                BinaryValue = null;
            }
            else
            {
                RawValue = StringValue ?? string.Empty;
                StringValue = null;
            }
        }

        #endregion Interception Points
    }
}