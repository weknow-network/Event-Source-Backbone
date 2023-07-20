namespace EventSourcing.Backbone.Tests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    public interface IVersionAwareBase
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