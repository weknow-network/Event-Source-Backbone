namespace EventSourcing.Backbone.UnitTests.Entities;

[EventsContract(EventsContractType.Producer)]
[EventsContract(EventsContractType.Consumer)]
public interface ISimple
{
    ValueTask Step1Async(int value);
    ValueTask Step2Async(int value);
}
