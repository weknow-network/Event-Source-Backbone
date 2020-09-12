
using System;
using System.Threading.Tasks;
#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerLifetime: IAsyncDisposable
    {
        /// <summary>
        /// Represent the consuming completion..
        /// </summary>
        Task Completion { get; }
    }
}