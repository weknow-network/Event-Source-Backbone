namespace EventSourcing.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
public interface IFlowAB : IFlowA, IFlowB
{
    ValueTask DerivedAsync(string key);
}
