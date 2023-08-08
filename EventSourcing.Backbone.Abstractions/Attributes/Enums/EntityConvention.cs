namespace EventSourcing.Backbone;

public enum EntityConvention
{
    /// <summary>
    /// Without separators. 
    /// </summary>
    None,
    /// <summary>
    /// With underline separators
    /// </summary>
    Underline,
    /// <summary>
    /// Default separators
    /// </summary>
    Default = Underline
}
