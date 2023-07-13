namespace EventSourcing.Backbone
{
    /// <summary>
    /// S3 provider options
    /// </summary>
    public record S3Options
    {
        public static readonly S3Options Default = new S3Options();

        public S3Options()
        {
            Bucket = null;
            BasePath = null;
            EnvironmentConvention = S3EnvironmentConvention.BucketPrefix;
        }

        /// <summary>
        /// The target bucket (see: UseEnvironmentConvension)
        /// </summary>
        public string? Bucket { get; init; }

        /// <summary>
        /// A suffix for the target bucket
        /// </summary>
        public string? BucketSuffix { get; init; } 

        /// <summary>
        /// Base path
        /// </summary>
        public string? BasePath { get; init; }

        /// <summary>
        /// Environment convention's options
        /// </summary>
        public S3EnvironmentConvention EnvironmentConvention { get; init; }

        /// <summary>
        /// Indicating whether to avoid actual writing (dry run).
        /// </summary>
        /// <remarks>
        /// Useful for migration of stream while keeping same storage.
        /// </remarks>
        public bool DryRun { get; init; } = false;
    }
}
