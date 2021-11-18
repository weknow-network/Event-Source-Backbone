namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// S3 provider options
    /// </summary>
    public record struct S3Options
    {
        /// <summary>
        /// The target bucket (see: UseEnvironmentConvension)
        /// </summary>
        public string? Bucket { get; init; }

        /// <summary>
        /// Base path
        /// </summary>
        public string? BasePath { get; init; }

        /// <summary>
        /// Environment convention's options
        /// </summary>
        public S3EnvironmentConvention EnvironmentConvension { get; init; } = S3EnvironmentConvention.None;
    }
}
