using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerFilterBuilder
    {
        /// <summary>
        /// Register raw interceptor.
        /// Intercept the consumer side execution before de-serialization.
        /// </summary>
        /// <param name="sourceShard">
        /// Specify the event source shard.
        /// Shard is a unique source name 
        /// which used for direct message channeling (routing).</param>
        /// <returns></returns>
        IConsumerHooksBuilder FromShard(string sourceShard);

        /// <summary>
        /// Register tag's channels, enable the consumer
        /// to get data from multiple sources (shards).
        /// For example: assuming that each order flow is written to
        /// unique source (shard).
        /// Register to ORDER tag will route all shards which holding 
        /// messages with ORDER tag to the consume.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        IConsumerHooksBuilder RegisterTags(
            params string[] tags);

        /// <summary>
        /// Register tag's channels, enable the consumer
        /// to get data from multiple sources (shards).
        /// For example: assuming that each order flow is written to
        /// unique source (shard).
        /// Register to ORDER tag will route all shards which holding 
        /// messages with ORDER tag to the consume.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        IConsumerHooksBuilder RegisterTags(
            IEnumerable[] tags);
    }
}
