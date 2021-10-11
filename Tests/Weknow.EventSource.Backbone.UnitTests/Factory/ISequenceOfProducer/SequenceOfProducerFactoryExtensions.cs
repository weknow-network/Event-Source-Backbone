using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    public static class SequenceOfProducerFactoryExtensions
    {
        public static ISequenceOfProducer BuildSequenceOfProducer(
            this IProducerSpecializeBuilder builder)
        {
            return builder.Build<ISequenceOfProducer>(plan => new SequenceOfProducerFactory(plan));
        }

        public static ISequenceOfProducer BuildSequenceOfProducer(
            this IProducerOverrideBuildBuilder<ISequenceOfProducer> builder)
        {
            return builder.Build(plan => new SequenceOfProducerFactory(plan));
        }
    }
}
