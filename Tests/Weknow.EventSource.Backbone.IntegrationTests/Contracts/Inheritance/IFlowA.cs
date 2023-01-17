namespace Weknow.EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
[Obsolete("Use for code generation")]
public interface IFlowA
{
    ValueTask AAsync(int id);
}
