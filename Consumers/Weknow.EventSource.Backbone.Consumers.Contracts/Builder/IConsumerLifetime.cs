using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerLifetime : IConsumerSubscribtionHubBuilder, IAsyncDisposable
    {
        /// <summary>
        /// Represent the consuming completion..
        /// </summary>
        Task Completion { get; }
    }
}