using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class GenerateEventSourceContractAttribute : Attribute
    {
        public GenerateEventSourceContractAttribute(EventSourceGenType generateType)
        {
            Type = generateType;
        }
        /// <summary>
        /// The name of the roducer interface.
        /// If missing the generator will use a convention.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Type of the generation
        /// </summary>
        public EventSourceGenType Type { get; }
    }
}
