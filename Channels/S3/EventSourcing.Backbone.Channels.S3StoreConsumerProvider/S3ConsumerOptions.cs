using System.Collections.Immutable;

namespace EventSourcing.Backbone;

public record S3ConsumerOptions : S3Options
{
    public static readonly S3ConsumerOptions Default = new S3ConsumerOptions();
}
