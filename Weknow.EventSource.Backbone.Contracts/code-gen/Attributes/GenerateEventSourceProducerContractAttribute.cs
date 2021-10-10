using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class GenerateEventSourceProducerContractAttribute : Attribute
    {
        /// <summary>
        /// The name of the roducer interface.
        /// If missing the generator will use a convention.
        /// </summary>
        public string? Name { get; init; }
    }
}
