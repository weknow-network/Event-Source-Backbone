namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Source raw producer.
    /// </summary>
    public interface IRawProducer
    {
        /// <summary>
        /// <![CDATA[Producer proxy for raw events sequence.
        /// Useful for data migration at the raw data level.]]>
        /// </summary>
        /// <returns></returns>
        ValueTask Produce(Announcement data);
    }
}
