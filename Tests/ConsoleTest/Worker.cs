using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

using EventSourcing.Backbone;

using FakeItEasy;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static ConsoleTest.Constants;

// see:
//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables

namespace ConsoleTest;

/// <summary>
/// Open telemetry extensions for ASP.NET Core
/// </summary>
internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFooProducer _producer;
    private readonly IConsumerLifetime _subscription;
    private readonly CancellationTokenSource _cancellation;

    public Worker(ILogger<Worker> logger,
        IFooProducer producer,
        IConsumerLifetime subscription,
        CancellationTokenSource cancellation)
    {
        _logger = logger;
        _producer = producer;
        _subscription = subscription;
        _cancellation = cancellation;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Console Test");

        Prepare(Subscriber);

        var sw = Stopwatch.StartNew();

        var snapshot = sw.Elapsed;
        _logger.LogInformation($"Build producer = {snapshot:mm\\:ss\\.ff}");
        snapshot = sw.Elapsed;

        var ab = new ActionBlock<int>(async i =>
        {
            await _producer.Event1Async();
            await _producer.Event2Async(i);
            await _producer.Event3Async($"e-{1}");
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 100 });
        for (int i = 0; i < MAX; i++)
        {
            ab.Post(i);
            //await _producer.Event1Async();
            //await _producer.Event2Async(i);
            //await _producer.Event3Async($"e-{1}");
        }
        ab.Complete();
        await ab.Completion;

        snapshot = sw.Elapsed - snapshot;
        _logger.LogInformation($"Produce = {snapshot:mm\\:ss\\.ff}");
        snapshot = sw.Elapsed;

        await _subscription.Completion;

        snapshot = sw.Elapsed - snapshot;
        _logger.LogInformation($"Consumed = {snapshot:mm\\:ss\\.ff}");

        try
        {
            A.CallTo(() => Subscriber.Event1Async(A<ConsumerContext>.Ignored))
                        .MustHaveHappened(MAX, Times.Exactly);
            A.CallTo(() => Subscriber.Event2Async(A<ConsumerContext>.Ignored, A<int>.Ignored))
                        .MustHaveHappened(MAX, Times.Exactly);
            A.CallTo(() => Subscriber.Event3Async(A<ConsumerContext>.Ignored, A<string>.Ignored))
                        .MustHaveHappened(MAX, Times.Exactly);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _logger.LogInformation(ex.FormatLazy());
            Console.ResetColor();
        }

        _logger.LogInformation("Done");
        _cancellation.CancelSafe();
    }


    static void Prepare(IFooConsumer subscriber)
    {
        A.CallTo(() => subscriber.Event1Async(A<ConsumerContext>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => subscriber.Event2Async(A<ConsumerContext>.Ignored, A<int>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
        A.CallTo(() => subscriber.Event3Async(A<ConsumerContext>.Ignored, A<string>.Ignored))
                .ReturnsLazily(() => ValueTask.CompletedTask);
    }
}
