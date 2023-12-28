using Amazon.S3;

using EventSourcing.Backbone.Building;
using EventSourcing.Backbone.Tests.Entities;

using FakeItEasy;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    [Trait("provider", "s3")]
    public sealed class S3StoreCredentialsTests : IDisposable
    {
        private const string TEST_URI = "testing";
        private readonly ITestOutputHelper _outputHelper;
        private readonly IHost _host;
        private const int TIMEOUT = 1000 * 20;
        private readonly ISimpleConsumer _subscriber = A.Fake<ISimpleConsumer>();

        #region Ctor

        public S3StoreCredentialsTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");
            _host = Host.CreateDefaultBuilder()
                            .ConfigureAppConfiguration((context, builder) =>
                            {
                                builder.SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                            })
                             //.AddEnvironmentVariables()
                             .ConfigureServices((context, services) =>
                             {
                                 var s3Options = new S3ConsumerOptions { Bucket = "event-sourcing-validation" };

                                 services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                                 services.AddAWSService<IAmazonS3>();
                                 //IHostEnvironment environment = context.HostingEnvironment;
                                 services.AddEventSourceRedisConnection();

                                 services.AddSingleton(ioc =>
                                 {
                                     var logger = ioc.GetService<Microsoft.Extensions.Logging.ILogger<S3StoreCredentialsTests>>();
                                     IProducerHooksBuilder producer = ioc.ResolveRedisProducerChannel()
                                        .ResolveS3Storage(s3Options)
                                        .WithLogger(logger!)
                                        .Uri(TEST_URI);

                                     return producer;
                                 });
                                 services.AddSingleton(ioc =>
                                 {
                                     //var logger = ioc.GetService<Microsoft.Extensions.Logging.ILogger<S3StoreCredentialsTests>>();
                                     IConsumerReadyBuilder consumer = ioc.ResolveRedisConsumerChannel()
                                        .ResolveS3Storage(s3Options)
                                        .Uri(TEST_URI);

                                     return consumer;
                                 });
                             })
                             .Build();
        }

        #endregion // Ctor

        #region S3_Cred

        [Fact(Timeout = TIMEOUT)]
        public async Task S3_Cred()
        {
            var producerBuilder = _host.Services.GetService<IProducerHooksBuilder>() ?? throw new EventSourcingException("Producer is null");

            ISimpleProducer producer = producerBuilder
                                            .EnvironmentFromVariable()
                                            .BuildSimpleProducer();

            var id1 = producer.Step1Async(1);
            var id2 = producer.Step2Async(2);

            IConsumerReadyBuilder consumerBuilder = _host.Services.GetService<IConsumerReadyBuilder>() ?? throw new EventSourcingException("Producer is null");
            IConsumerLifetime consumer = consumerBuilder
                            .EnvironmentFromVariable()
                            .WithOptions(opt => opt with { MaxMessages = 2 })
                            .SubscribeSimpleConsumer(_subscriber);

            await consumer.Completion;

            A.CallTo(() => _subscriber.Step1Async(A<ConsumerContext>.Ignored, 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.Step2Async(A<ConsumerContext>.Ignored, 2))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // S3_Cred

        #region S3_Cred_No_Env_Test

        //[Fact(Timeout = TIMEOUT)]
        [Fact]
        public async Task S3_Cred_No_Env_Test()
        {
            var producerBuilder = _host.Services.GetService<IProducerHooksBuilder>() ?? throw new EventSourcingException("Producer is null");

            ISimpleProducer producer = producerBuilder
                                            .BuildSimpleProducer();

            var id1 = producer.Step1Async(1);
            var id2 = producer.Step2Async(2);

            IConsumerReadyBuilder consumerBuilder = _host.Services.GetService<IConsumerReadyBuilder>() ?? throw new EventSourcingException("Producer is null");
            IConsumerLifetime consumer = consumerBuilder
                            .WithOptions(opt => opt with { MaxMessages = 2 })
                            .SubscribeSimpleConsumer(_subscriber);

            await consumer.Completion;

            A.CallTo(() => _subscriber.Step1Async(A<ConsumerContext>.Ignored, 1))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _subscriber.Step2Async(A<ConsumerContext>.Ignored, 2))
                .MustHaveHappenedOnceExactly();
        }

        #endregion // S3_Cred_No_Env_Test

        #region Dispose Pattern

        public void Dispose()
        {
            _host.Dispose();
            GC.SuppressFinalize(this);
        }

        ~S3StoreCredentialsTests()
        {
            Dispose();
        }

        #endregion // Dispose Pattern
    }
}
