using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly;

using StackExchange.Redis;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone.Tests
{
    public class S3StoreStrategyTests: EndToEndTests
    {
        const string DATE_FORMAT = "yyyy-MM-ddZHH-MM-ss";
        const string BUCKET_KEY = "S3_EVENT_SOURCE_BUCKET";
        private static readonly string BUCKET = Environment.GetEnvironmentVariable(BUCKET_KEY) ?? string.Empty;


        public S3StoreStrategyTests(ITestOutputHelper outputHelper): 
            base(outputHelper,
                (b, logger) => b.AddStorageStrategy(
                            new S3ProducerStorageStrategy(logger, BUCKET)),
                (b, logger) => b.AddStorageStrategy(
                            new S3ConsumerStorageStrategy(logger, BUCKET))
                )
        {
        }

    }
}
