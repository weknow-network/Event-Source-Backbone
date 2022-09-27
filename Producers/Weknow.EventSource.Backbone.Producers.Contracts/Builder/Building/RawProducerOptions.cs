
namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Raw producer options
    /// </summary>
    public record RawProducerOptions
    {
        /// <summary>
        /// Gets a value indicating whether keep original metadata.
        /// </summary>
        public bool KeepOriginalMeta { get; init; } = false;
    }
}
