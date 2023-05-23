using System.ComponentModel;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Mark for code generation
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class EventsContractAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="generateType">Type of the generate.</param>
        public EventsContractAttribute(EventsContractType generateType)
        {
            Type = generateType;
        }
        /// <summary>
        /// The name of the interface.
        /// If missing the generator will use a convention.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The Namespace.
        /// If missing the generator will use a convention.
        /// </summary>
        public string? Namespace { get; init; }

        /// <summary>
        /// Type of the generation
        /// </summary>
        public EventsContractType Type { get; }
    }
}
