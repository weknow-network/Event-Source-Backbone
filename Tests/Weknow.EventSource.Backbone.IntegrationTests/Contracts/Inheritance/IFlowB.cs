namespace Weknow.EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
[Obsolete("Use for code generation")]
public interface IFlowB
{
    ValueTask BAsync(DateTimeOffset date);
}
