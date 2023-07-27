namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event source's casting indicator, rewrite the parameter type
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class EventSourceFromTypeAttribute<T> : Attribute
    {
    }
}
