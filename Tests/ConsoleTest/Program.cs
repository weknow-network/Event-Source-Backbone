#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation

using EventSourcing.Backbone;
using System;
using System.Diagnostics;
using System.Text.Json;
using EventSourcing.Backbone.Building;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

using ConsoleTest;
using FakeItEasy;
using System.Threading.Tasks.Dataflow;
using EventSourcing.Backbone.Enums;

const int MAX = 8_000;
CancellationTokenSource cancellation = new CancellationTokenSource(
                                            Debugger.IsAttached
                                            ? TimeSpan.FromMinutes(10)
                                            : TimeSpan.FromSeconds(400));

string END_POINT_KEY = "REDIS_EVENT_SOURCE_ENDPOINT";

string URI = $"console-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}";

ILogger fakeLogger = A.Fake<ILogger>();
IFooConsumer subscriber = A.Fake<IFooConsumer>();

await Cleanup(END_POINT_KEY, URI, fakeLogger);

Prepare(subscriber);




Console.WriteLine("Console Test");

string ENV = $"console-test";

var sw = Stopwatch.StartNew();

IFooProducer producer = ProducerBuilder.Empty.UseRedisChannel()
                                .Environment(ENV)
                                .Uri(URI)
                                .BuildFooProducer();

var snapshot = sw.Elapsed;
Console.WriteLine($"Build producer = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

var ab = new ActionBlock<int>(async i =>
{
    await producer.Event1Async();
    await producer.Event2Async(i);
    await producer.Event3Async($"e-{1}");
}, new ExecutionDataflowBlockOptions {  MaxDegreeOfParallelism = 100 });
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
Console.WriteLine($"Produce = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

var consumerOptions = new ConsumerOptions
{
    AckBehavior = AckBehavior.OnSucceed,
    PartialBehavior = PartialConsumerBehavior.Loose,
    MaxMessages = MAX * 3 /* detach consumer after 2 messages*/
};

IConsumerSubscribeBuilder builder = ConsumerBuilder.Empty.UseRedisChannel()
             .WithOptions(o => consumerOptions)
                 .WithCancellation(cancellation.Token)
                 .Environment(ENV)
                 .Uri(URI);

snapshot = sw.Elapsed - snapshot;
Console.WriteLine($"Build Consumer = {snapshot:mm\\:ss\\.ff}");
snapshot = sw.Elapsed;

await using IConsumerLifetime subscription = builder
                            .Group("CONSUMER_GROUP_1")
                            .Name($"TEST {DateTime.UtcNow:HH:mm:ss}")
                            .SubscribeFooConsumer(subscriber);

await subscription.Completion;

snapshot = sw.Elapsed - snapshot;
Console.WriteLine($"Consumed = {snapshot:mm\\:ss\\.ff}");
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
    Console.WriteLine(ex.FormatLazy());
    Console.ResetColor();
}

Console.WriteLine("Done");

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