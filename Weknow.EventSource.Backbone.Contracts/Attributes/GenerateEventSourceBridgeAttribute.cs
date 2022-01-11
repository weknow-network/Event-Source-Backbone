using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [Obsolete("Deprecated, use GenerateEventSourceAttribute instead", true)]
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class GenerateEventSourceBridgeAttribute : GenerateEventSourceBaseAttribute
    {
        public GenerateEventSourceBridgeAttribute(EventSourceGenType generateType) : base(generateType)
        {
        }

        public string? InterfaceName { get; init; }
    }
}
