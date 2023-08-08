namespace EventSourcing.Backbone;

public record S3ConsumerOptions : S3Options
{
    public new static readonly S3ConsumerOptions Default = new S3ConsumerOptions();
}
