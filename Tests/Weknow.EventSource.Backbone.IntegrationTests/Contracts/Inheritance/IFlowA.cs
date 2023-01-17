namespace Weknow.EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
public interface IFlowA
{
    ValueTask AAsync(int id);
}
