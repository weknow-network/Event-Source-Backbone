using EventSource.Backbone.Building;

namespace EventSource.Backbone
{
    public interface IConsumerLifetime : IConsumerSubscribtionHubBuilder, IAsyncDisposable
    {
        /// <summary>
        /// Represent the consuming completion..
        /// </summary>
        Task Completion { get; }
    }
}