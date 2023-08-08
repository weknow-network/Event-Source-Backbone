using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    public static class SequenceOperationsProducerFactoryExtensions
    {
        public static ISequenceOfProducer BuildCustomSequenceOfProducer(
            this IProducerSpecializeBuilder builder)
        {
            return builder.Build<ISequenceOfProducer>(plan => new SequenceOperationsProducerFactory(plan));
        }
    }
}
