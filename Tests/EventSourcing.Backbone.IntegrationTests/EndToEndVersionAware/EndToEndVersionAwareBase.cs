using System.Collections.Concurrent;
using System.Threading.Channels;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Channels.RedisProvider;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;



namespace EventSourcing.Backbone.Tests;

public abstract class EndToEndVersionAwareBase : TestsBase
{
    protected readonly IProducerStoreStrategyBuilder _producerBuilder;
    protected readonly IConsumerStoreStrategyBuilder _consumerBuilder;

    #region Ctor

    protected EndToEndVersionAwareBase(
            ITestOutputHelper outputHelper,
            Func<IProducerStoreStrategyBuilder, ILogger, IProducerStoreStrategyBuilder>? producerChannelBuilder = null,
             Func<IConsumerStoreStrategyBuilder, ILogger, IConsumerStoreStrategyBuilder>? consumerChannelBuilder = null)
            : base(outputHelper)
    {
        _producerBuilder = ProducerBuilder.Empty.UseRedisChannel( /*,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster" */);
        _producerBuilder = producerChannelBuilder?.Invoke(_producerBuilder, _fakeLogger) ?? _producerBuilder;
        var stg = new RedisConsumerChannelSetting
        {
            DelayWhenEmptyBehavior = new DelayWhenEmptyBehavior
            {
                CalcNextDelay = ((d, _) => TimeSpan.FromMilliseconds(2))
            }
        };
        var consumerBuilder = stg.CreateRedisConsumerBuilder();
        _consumerBuilder = consumerChannelBuilder?.Invoke(consumerBuilder, _fakeLogger) ?? consumerBuilder;
    }

    #endregion // Ctor

    private readonly string URI_DYNAMIC  = $"{DateTime.UtcNow:HH_mm_ss}:{Environment.TickCount}";
    protected sealed override string URI  => $"version-aware-{Name}:{URI_DYNAMIC}";
    protected abstract string Name { get; }
}
