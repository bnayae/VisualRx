using System.Runtime.Serialization;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// on next marble
    /// </summary>
    [DataContract(Namespace = Constants.DataNamespace, IsReference = true)]
    public class MarbleNext : MarbleBase
    {
        private const MarbleSerializationOptions SERIALIZE_OPTION =
            MarbleSerializationOptions.NetDataContractSerialization | MarbleSerializationOptions.Serializable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarbleNext"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName">Name of the machine.</param>
        internal MarbleNext(
            string name,
            MarbleSerializationOptions options,
            TimeSpan elapsed,
            string machineName)
            : base(name, options, MarbleKind.OnNext, elapsed, machineName)
        {
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="item">The item.</param>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="machineName"></param>
        /// <returns></returns>
        public static MarbleBase Create<T>(
            string name,
            MarbleSerializationOptions options,
            T item,
            TimeSpan elapsed,
            string machineName)
        {
            var marble = new MarbleNext(name, options, elapsed, machineName);

            //object surrogate = null;
            //if (surrogateMapping == null)
            //    surrogate = item;
            //else 
            //    surrogate = surrogateMapping(item, marble) ?? item;

            if (item is string ||
                (options & SERIALIZE_OPTION) == MarbleSerializationOptions.None)
            {
                marble.RawValue = item; // TODO: consider BinaryFormatter / NetDataContractSerialization if supported
                marble.FormattedValue = item.ToString();
            }
            else
            {
                marble.RawValue = item;
            }
            return marble;
        }
    }

}