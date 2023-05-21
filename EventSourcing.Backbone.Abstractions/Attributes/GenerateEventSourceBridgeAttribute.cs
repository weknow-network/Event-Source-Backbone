namespace EventSourcing.Backbone
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
