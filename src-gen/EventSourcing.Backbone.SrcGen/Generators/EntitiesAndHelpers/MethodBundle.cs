using System.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

/// <summary>
/// Method Bundle
/// </summary>
[DebuggerDisplay("{Name}, {Version}, {Parameters}")]
internal sealed class MethodBundle : IFormattable
{
    public MethodBundle(MethodDeclarationSyntax method, string name, string fullName, int version, string parameters, bool excluded)
    {
        Method = method;
        Name = name;
        FullName = fullName;
        Version = version;
        Parameters = parameters;
        Deprecated = excluded;
    }

    public MethodDeclarationSyntax Method { get; }
    public string Name { get; }
    public string FullName { get; }
    public int Version { get; }
    public string Parameters { get; }
    public bool Deprecated { get; }

    public override string ToString() => $"{Name}_{Version}_{Parameters.Replace(",", "_")}";

    string IFormattable.ToString(string format, IFormatProvider formatProvider)
    {
        string fmt = format ?? "_";
        string ex = Deprecated ? "X " : string.Empty;
        return $"{ex}{Name}{fmt}{Version}{fmt}{Parameters.Replace(",", fmt)}";
    }
}
