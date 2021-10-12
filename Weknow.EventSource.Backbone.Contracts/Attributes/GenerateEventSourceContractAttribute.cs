using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class GenerateEventSourceContractAttribute : GenerateEventSourceBaseAttribute
    {
        public GenerateEventSourceContractAttribute(EventSourceGenType generateType) : base(generateType)
        {
        }
    }
}
