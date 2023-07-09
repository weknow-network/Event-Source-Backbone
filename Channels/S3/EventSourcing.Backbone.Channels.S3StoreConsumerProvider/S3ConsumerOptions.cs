using System.Collections.Immutable;

namespace EventSourcing.Backbone;

public record S3ConsumerOptions : S3Options
{
    public static readonly S3ConsumerOptions Default = new S3ConsumerOptions();
    /// <summary>
    /// Useful when having multi storage configuration.
    /// May use to implement storage splitting (separation of concerns) like in the case of GDPR .
    /// </summary>
    public Predicate<string>? KeysFilter { get; init; }
}
