using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

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
