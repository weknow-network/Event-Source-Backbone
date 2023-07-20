using System.Collections.Immutable;

using Xunit.Abstractions;

namespace EventSourcing.Backbone.Tests
{
    public class S3StoreSplittingStrategyTests : EndToEndTests
    {
        private readonly static ImmutableHashSet<string> _gdrpFilter = ImmutableHashSet.CreateRange(new[] {
            nameof(Entities.ISequenceOperationsProducer.LoginAsync),
            nameof(Entities.ISequenceOperationsProducer.RegisterAsync),
            nameof(Entities.ISequenceOperationsProducer.UpdateAsync)
        });
        private static readonly S3ConsumerOptions OPTIONS = new S3ConsumerOptions
        {
            EnvironmentConvention = S3EnvironmentConvention.BucketPrefix,
            BasePath = "tests"
        };

        public S3StoreSplittingStrategyTests(ITestOutputHelper outputHelper) :
                base(outputHelper,
                    (b, logger) => b
                        .AddS3Storage((meta, key) => _gdrpFilter.Contains(meta.Operation),
                                        OPTIONS with { BucketSuffix = "-private" })
                        .AddS3Storage((meta, key) => !_gdrpFilter.Contains(meta.Operation),
                                        OPTIONS),
                    (b, logger) => b
                            .AddS3Storage(OPTIONS)
                            .AddS3Storage(OPTIONS with { BucketSuffix = "-private" })
                    )
        {
        }

    }
}
