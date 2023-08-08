namespace EventSourcing.Backbone
{
    /// <summary>
    /// Enable to replace the default serialization
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serialize item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        ReadOnlyMemory<byte> Serialize<T>(T item);
        /// <summary>
        /// Deserialize  data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedData">The serialized data.</param>
        /// <returns></returns>
        T Deserialize<T>(ReadOnlyMemory<byte> serializedData);
    }
}