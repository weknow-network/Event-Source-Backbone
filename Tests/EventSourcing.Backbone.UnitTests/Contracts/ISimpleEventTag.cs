using System.ComponentModel;

namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// Test contract
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISimpleEventTag : ISimpleEvent
    {
    }
}
