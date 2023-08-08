#pragma warning disable S1133 // Deprecated code should be removed


namespace EventSourcing.Backbone.Tests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    [EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Default)]
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Default)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    public interface IVersionAwareNone //: IVersionAwareBase
    {
        ValueTask ExecuteAsync(string key, int value);
        [EventSourceVersion(1, Date = "2023-06-01", Remark = "sample of deprecation")]
        ValueTask ExecuteAsync(int value);

        [EventSourceVersion(2)]
        ValueTask ExecuteAsync(DateTime value);

        [EventSourceVersion(3)]
        ValueTask ExecuteAsync(string value);
        [EventSourceVersion(4)]
        ValueTask ExecuteAsync(TimeSpan value);
        [EventSourceVersion(3)]
        ValueTask NotIncludesAsync(string value);
    }
}



