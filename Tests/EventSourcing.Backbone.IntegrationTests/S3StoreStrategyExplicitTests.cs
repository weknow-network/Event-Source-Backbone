using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests;

[Trait("provider", "s3")]
public class S3StoreStrategyExplicitTests : EndToEndExplicitTests
{

    public S3StoreStrategyExplicitTests(ITestOutputHelper outputHelper) :
        base(outputHelper,
            (b, logger) => b.AddS3Storage(),
            (b, logger) => b.AddS3Storage())
    {
    }

}
