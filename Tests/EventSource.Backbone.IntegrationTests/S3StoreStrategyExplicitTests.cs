using Xunit.Abstractions;

namespace EventSource.Backbone.Tests
{
    public class S3StoreStrategyExplicitTests : EndToEndExplicitTests
    {

        public S3StoreStrategyExplicitTests(ITestOutputHelper outputHelper) :
            base(outputHelper,
                (b, logger) => b.AddS3Strategy(),
                (b, logger) => b.AddS3Strategy())
        {
        }

    }
}
