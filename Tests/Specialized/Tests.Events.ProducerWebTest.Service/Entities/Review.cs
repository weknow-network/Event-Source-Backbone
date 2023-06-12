using Tests.Events.WebTest.Abstractions;

namespace Tests.Events.ProducerWebTest.Service.Entities;

public record Review(Id id, params string[] notes);
