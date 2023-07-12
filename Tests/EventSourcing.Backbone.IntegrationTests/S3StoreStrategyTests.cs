using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    public class S3StoreStrategyTests : EndToEndTests
    {
        private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
        {
            EnvironmentConvention = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        public S3StoreStrategyTests(ITestOutputHelper outputHelper) :
                base(outputHelper,
                    (b, logger) => b.AddS3Storage(OPTIONS),
                    (b, logger) => b.AddS3Storage(OPTIONS))
        {
        }

    }
}
