namespace EventSourcing.Backbone
{
    /// <summary>
    /// Environment convention's options
    /// </summary>
    public enum S3EnvironmentConvention
    {
        /// <summary>
        /// No convention
        /// </summary>
        None,
        /// <summary>
        /// Environment as bucket prefix
        /// </summary>
        BucketPrefix,
        /// <summary>
        /// Environment as path prefix
        /// </summary>
        PathPrefix
    }
}
