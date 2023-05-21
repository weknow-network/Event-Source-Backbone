namespace EventSourcing.Backbone
{
    /// <summary>
    /// Plan routing identification
    /// </summary>
    public interface IPlanRoute
    {
        /// <summary>
        /// Environment (part of the stream key).
        /// </summary>
        Env Environment { get; }
        /// <summary>
        /// The stream identifier (the URI combined with the environment separate one stream from another)
        /// </summary>
        string Uri { get; }
    }
}