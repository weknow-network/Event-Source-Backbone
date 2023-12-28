using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests;

[Trait("provider", "s3")]
public class S3StoreStrategyStressTests : EndToEndStressTests
{
    private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
    {
        EnvironmentConvention = S3EnvironmentConvention.BucketPrefix,
        BasePath = "tests"
    };

    public S3StoreStrategyStressTests(ITestOutputHelper outputHelper) :
            base(outputHelper,
                (b, logger) => b.AddS3Storage(OPTIONS),
                (b, logger) => b.AddS3Storage(OPTIONS))
    {
    }
}
