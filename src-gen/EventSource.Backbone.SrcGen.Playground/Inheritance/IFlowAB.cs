namespace EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
public interface IFlowAB : IFlowA, IFlowB
{
}
