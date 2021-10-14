using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.UnitTests.Entities
{
    /// <summary>
    /// Test contract
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer, ContractOnly = true)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISimpleEvent
    {
        ValueTask ExecuteAsync(string key, int value);
        ValueTask RunAsync(int id, DateTime date);
    }
}
