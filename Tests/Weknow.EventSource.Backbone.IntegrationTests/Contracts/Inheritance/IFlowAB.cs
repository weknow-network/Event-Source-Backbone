namespace Weknow.EventSource.Backbone.UnitTests.Entities;

[GenerateEventSource(EventSourceGenType.Producer)]
[GenerateEventSource(EventSourceGenType.Consumer)]
[Obsolete("Use for code generation", true)]
public interface IFlowAB : IFlowA, IFlowB
{
    ValueTask ExecAsync(DateTimeOffset date);

}
