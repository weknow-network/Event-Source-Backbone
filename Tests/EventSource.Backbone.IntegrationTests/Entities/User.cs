namespace EventSource.Backbone.UnitTests.Entities
{
    public record User
    {
        /// <summary>
        /// GDPR the right to be forgotten
        /// </summary>
        public Personal? Eracure { get; init; }
        /// <summary>
        /// Non-personal data
        /// </summary>
        public Anonymous Details { get; init; } = new Anonymous { Hobbies = "Home", Id = 32 };

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string? Comment { get; init; }
    }

    /// <summary>
    /// GDPR the right to be forgotten
    /// </summary>
    public record Personal
    {
        public string Name { get; init; } = string.Empty;
        public string GovernmentId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Non-personal data
    /// </summary>
    public record Anonymous
    {
        public int Id { get; init; }
        public string Hobbies { get; init; } = string.Empty;
    }
}
