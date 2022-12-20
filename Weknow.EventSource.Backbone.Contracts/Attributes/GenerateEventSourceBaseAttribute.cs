using System.ComponentModel;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Mark for code generation
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GenerateEventSourceBaseAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateEventSourceBaseAttribute"/> class.
        /// </summary>
        /// <param name="generateType">Type of the generate.</param>
        public GenerateEventSourceBaseAttribute(EventSourceGenType generateType)
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
        public EventSourceGenType Type { get; }
    }
}
