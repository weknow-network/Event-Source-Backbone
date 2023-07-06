using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    public class S3StoreStrategyStressTests : EndToEndStressTests
    {
        private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
        {
            EnvironmentConvension = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        public S3StoreStrategyStressTests(ITestOutputHelper outputHelper) :
                base(outputHelper,
                    (b, logger) => b.AddS3Storage(OPTIONS),
                    (b, logger) => b.AddS3Storage(OPTIONS))
        {
        }
    }
}
