using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: [bnaya 2021-10] generics attribute (.NET 6 migration)
namespace Weknow.EventSource.Backbone
{

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class GenerateEventSourceBridgeAttribute : Attribute
    {
        public GenerateEventSourceBridgeAttribute(EventSourceGenType generateType, Type contract)
        {
            GenerateType = generateType;
            Contract = contract;
        }

        /// <summary>
        /// Gets the interface contract.
        /// </summary>
        public Type Contract { get; }

        /// <summary>
        /// The name of the of the Extension method (without the build prefix).
        /// If missing the generator will use a convention.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The namespace.
        /// If missing the generator will use same namespace as the interface.
        /// </summary>
        public string? Namespace { get; init; }

        /// <summary>
        /// When true, generate auto suffix for the generated interface name (Producer / Consumer according to the ctor's generateType)
        /// </summary>
        public bool AutoSuffix { get; init; }

        /// <summary>
        /// Type of the generation (default = false)
        /// </summary>
        public EventSourceGenType GenerateType { get; }
    }
}
