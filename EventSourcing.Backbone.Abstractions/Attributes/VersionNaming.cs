namespace EventSourcing.Backbone;

public enum VersionNaming
{
    /// <summary>
    /// Keep original method name. 
    /// </summary>
    None,
    /// <summary>
    /// Append version suffix to the method name
    /// </summary>
    Append,
    /// <summary>
    /// Append version suffix to the method name separated with _
    /// </summary>
    AppendUnderscore,
    /// <summary>
    /// Append version suffix to the method name separated with _
    /// </summary>
    Default = None
}
