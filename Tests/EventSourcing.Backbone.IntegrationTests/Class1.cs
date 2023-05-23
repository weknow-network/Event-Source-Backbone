﻿
#nullable enable

using System.CodeDom.Compiler;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// Entity mapper is responsible of mapping announcement to DTO generated from SequenceOperationsConsumer
    /// </summary>
    /// <inheritdoc cref="ISequenceOperations" />
    [GeneratedCode("EventSourcing.Backbone.SrcGen", "1.1.141.0")]
    public static class SequenceOperationsConsumerEntityMapperExtensions1
    {
        /// <summary>
        /// Specialize Enumerator of event produced by ISequenceOperationsConsumer
        /// </summary>
        public static IConsumerIterator<ISequenceOperationsConsumer_EntityFamily> SpecializeSequenceOperationsConsumer(IConsumerIterator iterator) =>
               iterator.Specialize(SequenceOperationsConsumerEntityMapper.Default);
    }

}