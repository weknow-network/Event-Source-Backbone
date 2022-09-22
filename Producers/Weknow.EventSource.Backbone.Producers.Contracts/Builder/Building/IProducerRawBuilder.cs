namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source raw producer builder.
    /// </summary>
    public interface IProducerRawBuilder 
    {
        /// <summary>
        /// <![CDATA[ Ceate Producer proxy for raw events sequence.
        /// Useful for data migration at the raw data level.]]>
        /// </summary>
        /// <returns></returns>
        IRawProducer BuildRaw(RawProducerOptions? options = null);
    }
}
