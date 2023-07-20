namespace EventSourcing.Backbone.UnitTests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    [EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Default, IgnoreVersion = new[] { 3 })]
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Default, IgnoreVersion = new[] { 3 })]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    public interface IVersionAwareNone //: IVersionAwareBase
    {
        //[EventSourceVersion(Retired = 4)]
        ValueTask ExecuteAsync(string key, int value);
        [EventSourceVersion(1, Retired = 2)]
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



