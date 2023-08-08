namespace EventSourcing.Backbone.UnitTests.Entities
{
    /// <summary>
    /// The user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// GDPR the right to be forgotten
        /// </summary>
        public Personal Eracure { get; set; } = new Personal();
        /// <summary>
        /// Non-personal data
        /// </summary>
        public Anonymous Details { get; set; } = new Anonymous();

        /// <summary>
        /// GDPR the right to be forgotten
        /// </summary>
        public class Personal
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// Gets or sets the government id.
            /// </summary>
            public string GovernmentId { get; set; } = string.Empty;
        }

        /// <summary>   
        /// Non-personal data
        /// </summary>
        public class Anonymous
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Gets or sets the hobbies.
            /// </summary>
            public string Hobbies { get; set; } = string.Empty;
        }
    }
}
