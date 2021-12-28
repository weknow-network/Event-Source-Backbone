using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Mark for code generation
    /// </summary>
    /// <seealso cref="Weknow.EventSource.Backbone.GenerateEventSourceBaseAttribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class GenerateEventSourceAttribute : GenerateEventSourceBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateEventSourceAttribute"/> class.
        /// </summary>
        /// <param name="generateType">Type of the generate.</param>
        public GenerateEventSourceAttribute(EventSourceGenType generateType) : base(generateType)
        {
        }

        /// <summary>
        /// When true, it will only generate the contract interface(i.e. won't generate the bridge).
        /// </summary>
        public bool ContractOnly { get; init; }
    }
}
