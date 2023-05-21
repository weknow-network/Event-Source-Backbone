namespace EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
public interface IFlowB
{
    ValueTask BAsync(DateTimeOffset date);
}
