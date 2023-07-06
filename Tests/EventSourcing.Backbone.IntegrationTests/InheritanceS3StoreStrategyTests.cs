using System.Text;

using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    public class InheritanceS3StoreStrategyTests : InheritanceTests
    {
        private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
        {
            EnvironmentConvension = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        public InheritanceS3StoreStrategyTests(ITestOutputHelper outputHelper) :
                base(outputHelper,
                    (b, logger) => b.AddS3Storage(OPTIONS),
                    (b, logger) => b.AddS3Storage(OPTIONS))
        {
        }

    }
}
