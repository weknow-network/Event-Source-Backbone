using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Min: {MinVersion}, naming: {VersionNaming}")]
internal struct VersionInstructions
{
    public VersionInstructions()
    {
        VersionNaming = VersionNaming.Default;
        MinVersion = 0;
        EntityConvention = EntityConvention.Underline;
    }

    /// <summary>
    /// Version naming convention.
    /// </summary>
    public VersionNaming VersionNaming { get; set; }

    /// <summary>
    /// Version naming convention.
    /// </summary>
    public EntityConvention EntityConvention { get; set; }

    /// <summary>
    /// Won't generate method with version lower than this value
    /// </summary>
    public int MinVersion { get; set; }

    public string FormatMethodName(string name, int version)
    {
        string versionSuffix = VersionNaming switch
        {
            SrcGen.Entities.VersionNaming.Append => version.ToString(),
            SrcGen.Entities.VersionNaming.AppendUnderscore => $"_{version}",
            _ => string.Empty
        };

        if (name.EndsWith("Async"))
        {
            var prefix = name.Substring(0, name.Length - 5);
            return $"{prefix}{versionSuffix}Async";
        }
        return $"{name}{versionSuffix}";
    }
}
