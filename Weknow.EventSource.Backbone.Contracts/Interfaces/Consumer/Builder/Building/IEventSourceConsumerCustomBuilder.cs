namespace Weknow.EventSource.Backbone.Building
{
    public interface IEventSourceConsumerCustomBuilder: IEventSourceConsumer2Builder
    {
        /// <summary>
        /// Custom source.
        /// </summary>
        /// <param name="customSource">The custom source should be used carefully,
        /// and only when the data shouldn't be sequence with other sources.</param>
        /// <returns></returns>
        IEventSourceConsumer2Builder CustomSource(
                                    string customSource);
    }
}