namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event source's version deprecation.
    /// Used to retired an API version
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EventSourceDeprecateVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventsContractAttribute" /> class.
        /// </summary>
        /// <param name="type">The event target type.</param>
        /// <param name="versionOfDeprecation">The version in which the method was deprecated</param>
        public EventSourceDeprecateVersionAttribute(EventsContractType type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the target type.
        /// </summary>
        public EventsContractType Type { get; }

        /// <summary>
        /// Describe the deprecation reason
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// Document the date of deprecation, recommended format is: yyyy-MM-dd
        /// </summary>
        public string Date { get; set; } = string.Empty;
    }
}
