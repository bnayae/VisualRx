using System.Runtime.Serialization;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// Marble error
    /// </summary>
    [DataContract(Namespace = Constants.DataNamespace, IsReference = true)]
    public class MarbleError : MarbleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleError"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        internal MarbleError(string name, MarbleSerializationOptions options,
            Exception ex, TimeSpan elapsed, string machineName)
            : base(name, options, MarbleKind.OnError, elapsed, machineName)
        {
            base.RawValue = ex.ToString();
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <returns></returns>
        public static MarbleBase Create(string name,
            MarbleSerializationOptions options, Exception ex, TimeSpan elapsed,
            string machineName)
        {
            return new MarbleError(name, options, ex, elapsed, machineName);
        }
    }
}