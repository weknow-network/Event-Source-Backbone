﻿namespace EventSourcing.Backbone.Tests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    [EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.AppendUnderscore)]
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.AppendUnderscore)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    public interface IVersionAwareAppendUnderscore//: IVersionAwareBase
    {
        //[EventSourceVersion(Retired = 4)]
        ValueTask ExecuteAsync(string key, int value);
        [EventSourceVersion(1)]
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
