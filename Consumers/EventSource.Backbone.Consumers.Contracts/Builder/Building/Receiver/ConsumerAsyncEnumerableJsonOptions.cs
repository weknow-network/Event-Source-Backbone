namespace EventSource.Backbone
{
    /// <summary>
    /// Receive data (on demand data query).
    /// </summary>
    public record ConsumerAsyncEnumerableJsonOptions : ConsumerAsyncEnumerableOptions

    {
        /// <summary>
        /// Ignore metadata.
        /// </summary>
        public bool IgnoreMetadata { get; init; }
    }
}
