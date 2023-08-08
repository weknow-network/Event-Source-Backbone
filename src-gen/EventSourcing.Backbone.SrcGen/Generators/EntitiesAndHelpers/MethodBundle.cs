using System.Diagnostics;

using EventSourcing.Backbone.SrcGen.Entities;

using Microsoft.CodeAnalysis;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

/// <summary>
/// Method Bundle
/// </summary>
[DebuggerDisplay("{Name}, {Version}, {Parameters}, Deprecated: {Deprecated}")]
internal sealed class MethodBundle : IFormattable
{
    private readonly VersionInstructions _versionInfo;

    public MethodBundle(
        IMethodSymbol method,
        string name,
        string fullName,
        int version,
        VersionInstructions versionInfo,
        string parameters,
        bool deprecated)
    {
        Method = method;
        Name = name;
        FullName = fullName;
        Version = version;
        _versionInfo = versionInfo;
        VersionNaming = versionInfo.VersionNaming;
        Parameters = parameters;
        Deprecated = deprecated;
    }

    public IMethodSymbol Method { get; }
    public string Name { get; }
    public string FullName { get; }
    public int Version { get; }
    public VersionNaming VersionNaming { get; }
    public string Parameters { get; }
    public bool Deprecated { get; }

    public override string ToString() => $"{Name}_{Version}_V{Parameters.Replace(",", "_")}";

    string IFormattable.ToString(string format, IFormatProvider formatProvider)
    {
        string fmt = format switch
        {
            "entity" => _versionInfo.EntityConvention switch
            {
                EntityConvention.None => string.Empty,
                _ => "_"
            },
            "e" => _versionInfo.EntityConvention switch
            {
                EntityConvention.None => string.Empty,
                _ => "_"
            },
            _ => null
        } ?? VersionNaming switch
        {
            VersionNaming.None => string.Empty,
            _ => "_"
        };
        if (string.IsNullOrEmpty(Parameters))
            return $"{FullName}{fmt}V{Version}";
        else
            return $"{FullName}{fmt}V{Version}{fmt}{Parameters.Replace(",", fmt)}";
    }

    public string FormatMethodFullName(string? nameOverride = null)
    {
        string name = nameOverride ?? FullName;
        string versionSuffix = VersionNaming switch
        {
            SrcGen.Entities.VersionNaming.Append => Version.ToString(),
            SrcGen.Entities.VersionNaming.AppendUnderscore => $"_{Version}",
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
