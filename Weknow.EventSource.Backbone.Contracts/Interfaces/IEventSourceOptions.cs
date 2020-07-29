using System;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// options
    /// </summary>
    public interface IEventSourceOptions
    {
        /// <summary>
        /// Gets the serializer.
        /// </summary>
        IDataSerializer Serializer { get; }
    }
}