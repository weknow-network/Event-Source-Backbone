using Tests.Events.WebTest.Abstractions;

namespace Tests.Events.ProducerWebTest.Service.Entities;

public record Test(Id id, params string[] notes);
