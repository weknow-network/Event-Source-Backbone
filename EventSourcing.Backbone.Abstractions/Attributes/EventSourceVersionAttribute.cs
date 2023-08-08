namespace EventSourcing.Backbone;

/// <summary>
/// Event source's version control.
/// Best practice when changing version is:
/// - when compatible: to increment the version number and add ConsumeFrom.
/// - when not compatible: to add new overload with increment version.
/// We don't expect a gap between ConsumeFrom to a lower version.
/// Versions expect to start at 0, if no version specified It will consider version 0,
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EventSourceVersionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventsContractAttribute" /> class.
    /// </summary>
    /// <param name="version">The version which will be produce.</param>
    public EventSourceVersionAttribute(int version = 0)
    {
        Version = version;
    }


    /// <summary>
    /// Gets the version which will be produce.
    /// When From doesn't specified this version will consider as the lower version for consume.
    /// When UpTo doesn't specified this version will consider as the higher version for consume.
    /// </summary>
    public int Version { get; }


    /// <summary>
    /// Describe the deprecation reason
    /// </summary>
    public string Remark { get; set; } = string.Empty;

    /// <summary>
    /// Document the date of deprecation, recommended format is: yyyy-MM-dd
    /// </summary>
    public string Date { get; set; } = string.Empty;
}
