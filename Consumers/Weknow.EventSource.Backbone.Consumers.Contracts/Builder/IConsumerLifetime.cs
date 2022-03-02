
using System;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerLifetime: IConsumerSubscribtionHubBuilder, IAsyncDisposable
    {
        /// <summary>
        /// Represent the consuming completion..
        /// </summary>
        Task Completion { get; }
    }
}