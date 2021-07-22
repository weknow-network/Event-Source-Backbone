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
    public class S3StoreStrategyExplicitTests: EndToEndExplicitTests
    {

        public S3StoreStrategyExplicitTests(ITestOutputHelper outputHelper): 
            base(outputHelper,
                (b, logger) => b.AddS3Strategy(logger),
                (b, logger) => b.AddS3Strategy(logger))
        {
        }

    }
}
