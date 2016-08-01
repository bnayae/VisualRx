using System.Runtime.Serialization;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// complete marble
    /// </summary>
    [DataContract(Namespace = Constants.DataNamespace, IsReference = true)]
    public class MarbleComplete : MarbleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleComplete"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        internal MarbleComplete(string name, MarbleSerializationOptions options, TimeSpan elapsed,
            string machineName)
            : base(name, options, MarbleKind.OnCompleted, elapsed, machineName)
        {
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        /// <returns></returns>
        public static MarbleBase Create(string name, MarbleSerializationOptions options,
            TimeSpan elapsed, string machineName)
        {
            return new MarbleComplete(name, options, elapsed, machineName);
        }
    }
}