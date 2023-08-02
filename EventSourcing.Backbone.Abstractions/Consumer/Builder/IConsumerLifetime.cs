using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone
{
    public interface IConsumerLifetime:  IAsyncDisposable
    {
        /// <summary>
        /// Represent the consuming completion..
        /// </summary>
        Task Completion { get; }
    }
}