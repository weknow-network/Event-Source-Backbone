using Xunit;
using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests;

[Trait("provider", "s3")]
public class S3FallbackTests : EndToEndVersionAware_Fallback_Tests
{

    public S3FallbackTests(ITestOutputHelper outputHelper) :
        base(outputHelper,
            (b, logger) => b.AddS3Storage(),
            (b, logger) => b.AddS3Storage())
    {
    }

    protected override string Name { get; } = "s3-fallback";
}
