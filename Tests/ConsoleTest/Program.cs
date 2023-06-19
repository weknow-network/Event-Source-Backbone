#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0301 // Closure Allocation Source

using EventSourcing.Backbone;
using System;
using System.Diagnostics;
using System.Text.Json;
using EventSourcing.Backbone.Building;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

using ConsoleTest;
using FakeItEasy;
using System.Threading.Tasks.Dataflow;
using EventSourcing.Backbone.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

const int MAX = 1_000;
CancellationTokenSource cancellation = new CancellationTokenSource(
                                            Debugger.IsAttached
                                            ? TimeSpan.FromMinutes(10)
                                            : TimeSpan.FromSeconds(400));

string END_POINT_KEY = "REDIS_EVENT_SOURCE_ENDPOINT";

string URI = $"console-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}";

string ENV = $"console-test";

IFooConsumer subscriber = A.Fake<IFooConsumer>();

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());
services.AddEventSourceRedisConnection();
services.AddSingleton(ioc =>
{
    IFooProducer producer = ioc.ResolveRedisProducerChannel()
                        .Environment(ENV)
                        .Uri(URI)
                        .BuildFooProducer();
    return producer;
});
services.AddSingleton(ioc =>
{
    var consumerOptions = new ConsumerOptions
    {
        AckBehavior = AckBehavior.OnSucceed,
        PartialBehavior = PartialConsumerBehavior.Loose,
        MaxMessages = MAX * 3 /* detach consumer after 2 messages*/
    };
    IConsumerLifetime subscription =
               ioc.ResolveRedisConsumerChannel()
             .WithOptions(o => consumerOptions)
                 .WithCancellation(cancellation.Token)
                 .Environment(ENV)
                 .Uri(URI)
                 .Group("CONSUMER_GROUP_1")
                 .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                  .SubscribeFooConsumer(subscriber);
    return subscription;
});

var sp = services.BuildServiceProvider();
ILogger logger = sp.GetService<ILogger<Program>>() ?? throw new NullReferenceException();

logger.LogInformation("Console Test");

await Cleanup(END_POINT_KEY, URI, logger);

Prepare(subscriber);


IFooProducer producer = sp.GetService<IFooProducer>() ?? throw new NullReferenceException();
IConsumerLifetime subscription = sp.GetService<IConsumerLifetime>() ?? throw new NullReferenceException();


var sw = Stopwatch.StartNew();

var snapshot = sw.Elapsed;
logger.LogInformation($"Build producer = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

var ab = new ActionBlock<int>(async i =>
{
    await producer.Event1Async();
    await producer.Event2Async(i);
    await producer.Event3Async($"e-{1}");
}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 100 });
for (int i = 0; i < MAX; i++)
{
    ab.Post(i);
    //await producer.Event1Async();
    //await producer.Event2Async(i);
    //await producer.Event3Async($"e-{1}");
}
ab.Complete();
await ab.Completion;

snapshot = sw.Elapsed - snapshot;
logger.LogInformation($"Produce = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

await subscription.Completion;

snapshot = sw.Elapsed - snapshot;
logger.LogInformation($"Consumed = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

try
{
    A.CallTo(() => subscriber.Event1Async(A<ConsumerMetadata>.Ignored))
                .MustHaveHappened(MAX, Times.Exactly);
    A.CallTo(() => subscriber.Event2Async(A<ConsumerMetadata>.Ignored, A<int>.Ignored))
                .MustHaveHappened(MAX, Times.Exactly);
    A.CallTo(() => subscriber.Event3Async(A<ConsumerMetadata>.Ignored, A<string>.Ignored))
                .MustHaveHappened(MAX, Times.Exactly);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    logger.LogInformation(ex.FormatLazy());
    Console.ResetColor();
}

logger.LogInformation("Done");

static async Task Cleanup(string END_POINT_KEY, string URI, ILogger fakeLogger)
{
    IConnectionMultiplexer conn = RedisClientFactory.CreateProviderAsync(
                                        logger: fakeLogger,
                                        configurationHook: cfg => cfg.AllowAdmin = true).Result;
    string serverNames = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
    foreach (var serverName in serverNames.Split(','))
    {
        var server = conn.GetServer(serverName);
        if (!server.IsConnected)
            continue;
        IEnumerable<RedisKey> keys = server.Keys(pattern: $"*{URI}*");
        IDatabaseAsync db = conn.GetDatabase();

        var ab = new ActionBlock<string>(k => db.KeyDeleteAsync(k, CommandFlags.DemandMaster), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 30 });
        foreach (string key in keys)
        {
            ab.Post(key);
        }

        ab.Complete();
        await ab.Completion;
    }
}

static void Prepare(IFooConsumer subscriber)
{
    A.CallTo(() => subscriber.Event1Async(A<ConsumerMetadata>.Ignored))
            .ReturnsLazily(() => ValueTask.CompletedTask);
    A.CallTo(() => subscriber.Event2Async(A<ConsumerMetadata>.Ignored, A<int>.Ignored))
            .ReturnsLazily(() => ValueTask.CompletedTask);
    A.CallTo(() => subscriber.Event3Async(A<ConsumerMetadata>.Ignored, A<string>.Ignored))
            .ReturnsLazily(() => ValueTask.CompletedTask);
}