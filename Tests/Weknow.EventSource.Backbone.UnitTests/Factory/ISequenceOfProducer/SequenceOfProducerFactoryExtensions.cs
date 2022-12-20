using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public static class SequenceOfProducerFactoryExtensions
    {
        public static ISequenceOfProducer CustomBuildSequenceOfProducer(
            this IProducerSpecializeBuilder builder)
        {
            return builder.Build<ISequenceOfProducer>(plan => new SequenceOfProducerFactory(plan));
        }

        public static ISequenceOfProducer CustomBuildSequenceOfProducer(
            this IProducerOverrideBuildBuilder<ISequenceOfProducer> builder)
        {
            return builder.Build(plan => new SequenceOfProducerFactory(plan));
        }
    }
}
