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
        public EventSourceDeprecateVersionAttribute(EventsContractType type, int versionOfDeprecation)
        {
            Type = type;
            Version = versionOfDeprecation;
        }

        /// <summary>
        /// Document when on which version the method had considered deprecated.
        /// It doesn't affect the deprecation itself, when having a deprecation attribute on a method it become deprecated no matters the version number.
        /// </summary>
        /// <summary>
        /// Indicates the version in which the method was considered deprecated (as a structured documentation).
        /// The deprecation itself is not affected by this documentation; 
        /// once a deprecation attribute is added to a method, 
        /// it becomes deprecated regardless of the version number specified.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets the target type.
        /// </summary>
        public EventsContractType Type { get; }

        /// <summary>
        /// Describe the deprecation reason
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// Document the date of deprecation, recommended format is: yyyy-MM-dd
        /// </summary>
        public string Date { get; set; }
    }
}
