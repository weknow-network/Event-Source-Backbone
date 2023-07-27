using System.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

/// <summary>
/// Method Bundle
/// </summary>
[DebuggerDisplay("{Name}, {Version}, {Parameters}")]
internal sealed class MethodBundle : IFormattable
{
    public MethodBundle(MethodDeclarationSyntax method, string name, int version, string parameters)
    {
        Method = method;
        Name = name;
        Version = version;
        Parameters = parameters;
    }

    public MethodDeclarationSyntax Method { get; }
    public string Name { get; }
    public int Version { get; }
    public string Parameters { get; }

    public override string ToString() => $"{Name}_{Version}_{Parameters.Replace(",", "_")}";

    string IFormattable.ToString(string format, IFormatProvider formatProvider)
    {
        string fmt = format ?? "_";
        return $"{Name}{fmt}{Version}{fmt}{Parameters.Replace(",", fmt)}";
    }
}
