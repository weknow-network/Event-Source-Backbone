using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("{Type}, Min: {MinVersion}, naming: {VersionNaming}")]
internal struct VersionInfo
{
    public VersionInfo(EventsContractType type)
    {
        Type = type;
        VersionNaming = VersionNaming.Default;
        MinVersion = 0;
        IgnoreVersion = Array.Empty<int>();
    }

    public EventsContractType Type { get; private set; }

    /// <summary>
    /// Version naming convention.
    /// </summary>
    public VersionNaming VersionNaming { get; set; }

    /// <summary>
    /// Won't generate method with version lower than this value
    /// </summary>
    public int MinVersion { get; set; }

    /// <summary>
    /// Won't generate method with versions specified
    /// </summary>
    public int[] IgnoreVersion { get; set; }
}
