
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GenerateEventSourceBaseAttribute : Attribute
    {
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
