using System;

namespace Weknow.EventSource.Backbone
{

    /// <summary>
    /// Represent hints for event execution.
    /// Some execution content (like reconstruction) might like to ignore
    /// event with specific flags.
    /// </summary>
    [Flags]
    public enum EventFlags
    {
        None,
        Reconstruct
    }
}
