﻿#pragma warning disable S1133 // Deprecated code should be removed

using EventSourcing.Backbone;

namespace Tests.Events.WebTest.Abstractions;

/// <summary>
/// Event's schema definition
/// Return type of each method should be  <see cref="System.Threading.Tasks.ValueTask"/>
/// </summary>
[EventsContract(EventsContractType.Consumer)]
[EventsContract(EventsContractType.Producer)]
[Obsolete("Either use the Producer or Consumer version of this interface", true)]
public interface IProductCycle
{
    ValueTask IdeaAsync(string title, string describe);
    ValueTask PlanedAsync(string id, Version version, string doc);
    ValueTask ReviewedAsync(string id, Version version, params string[] notes);
    ValueTask ImplementedAsync(string id, Version version);
    ValueTask TestedAsync(string id, Version version, params string[] notes);
    ValueTask DeployedAsync(string id, Version version);
    ValueTask RejectedAsync(string id, Version version, string operation, NextStage nextStage, params string[] notes);
}