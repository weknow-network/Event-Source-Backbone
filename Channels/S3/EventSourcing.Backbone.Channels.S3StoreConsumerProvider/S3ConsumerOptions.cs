using System.Collections.Immutable;

namespace EventSourcing.Backbone;

public record S3ConsumerOptions : S3Options
{
    public static readonly S3ConsumerOptions Default = new S3ConsumerOptions();
    /// <summary>
    /// Useful when having multi storage configuration.
    /// May use to implement storage splitting like in the case of GDPR .
    /// </summary>
    public Predicate<string>? KeysFilter { get; init; }

    /// <summary>
    /// Indicating whether to override an existing key if it already exists in the bucket (multi storage cache scenario)
    /// </summary>
    /// <value>
    ///   <c>true</c> to override; otherwise, <c>false</c>.
    /// </value>
    public bool OverrideKeyIfExists { get; init; }
}
