using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    public class InheritanceS3StoreStrategyTests : InheritanceTests
    {
        private static readonly S3Options OPTIONS = new S3Options
        {
            EnvironmentConvension = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        public InheritanceS3StoreStrategyTests(ITestOutputHelper outputHelper) :
                base(outputHelper,
                    (b, logger) => b.AddS3Strategy(OPTIONS),
                    (b, logger) => b.AddS3Strategy(OPTIONS))
        {
        }

    }
}
