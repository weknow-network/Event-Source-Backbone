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
using static ConsoleTest.Constants;

CancellationTokenSource cancellation = new CancellationTokenSource(
                                            Debugger.IsAttached
                                            ? TimeSpan.FromMinutes(10)
                                            : TimeSpan.FromSeconds(400));
string URI = $"console-{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}";
IHostBuilder host = Host.CreateDefaultBuilder(args)
         .ConfigureServices(services =>
         {
             services.AddLogging(b => b.AddConsole());
             services.AddEventSourceRedisConnection();
             services.AddSingleton(cancellation);
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
                               .SubscribeFooConsumer(Subscriber);
                 return subscription;
             });
             services.AddOpenTelemetryEventSourcing(ENV);

             services.AddHostedService<Worker>();
         });

IHost hosing = host.Build();
ILogger _logger = hosing.Services.GetService<ILogger<Program>>() ?? throw new NullReferenceException();
await Cleanup(END_POINT_KEY, URI, _logger);

await hosing.RunAsync(cancellation.Token);

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