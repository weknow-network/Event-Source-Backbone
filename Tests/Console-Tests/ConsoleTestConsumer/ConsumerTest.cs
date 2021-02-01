using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

using static Weknow.EventSource.Backbone.ConsoleTests.Constants;

namespace Weknow.EventSource.Backbone.ConsoleTests
{
    static class ConsumerTest
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"{PARTITION}: {SHARD_A}");
            var consumerBuilder = ConsumerBuilder.Empty.UseRedisConsumerChannel(
                                        CancellationToken.None,
                                        redisConfiguration: (cfg) => cfg.ServiceName = "mymaster");


            var consumerOptions = new ConsumerOptions(AckBehavior.OnSucceed);


            await using IConsumerLifetime subscription = consumerBuilder
                             .WithOptions(consumerOptions)
                             .WithCancellation(CancellationToken.None)
                             .Partition(PARTITION)
                             .Shard(SHARD_A)
                             .Subscribe(meta => Subscriber.Default, "CONSUMER_GROUP_1", $"TEST {DateTime.UtcNow:HH:mm:ss}");
            Console.WriteLine("Consuming...");
            Console.ReadKey(true);
        }

    }
}
