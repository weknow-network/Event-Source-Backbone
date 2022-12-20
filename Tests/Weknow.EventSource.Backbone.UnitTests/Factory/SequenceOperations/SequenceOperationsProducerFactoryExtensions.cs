﻿using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public static class SequenceOperationsProducerFactoryExtensions
    {
        public static ISequenceOperationsProducer BuildCustomSequenceOperationsProducer(
            this IProducerSpecializeBuilder builder)
        {
            return builder.Build<ISequenceOperationsProducer>(plan => new SequenceOperationsProducerFactory(plan));
        }
    }
}
