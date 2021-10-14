using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class GenerateEventSourceAttribute : GenerateEventSourceBaseAttribute
    {
        public GenerateEventSourceAttribute(EventSourceGenType generateType) : base(generateType)
        {
        }

        /// <summary>
        /// When true, it will only generate the contract interface(i.e. won't generate the bridge).
        /// </summary>
        public bool ContractOnly { get; init; }
    }
}
