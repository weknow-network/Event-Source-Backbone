using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Immutable;
using EventSourcing.Backbone.SrcGen.Entities;

namespace EventSourcing.Backbone;

/// </summary>
internal static class Helper
{
    #region Convert

    /// <summary>
    /// Converts the specified text.
    /// </summary>
    /// <param name="txt">The text.</param>
    /// <param name="kind">The kind.</param>
    /// <returns></returns>
    internal static string Convert(string txt, string kind)
    {
        if (kind == "Producer")
        {
            return txt.Replace("Consumer", "Producer")
                                         .Replace("Subscriber", "Publisher")
                                         .Replace("Sub", "Pub").Trim();
        }
        if (kind == "Consumer")
        {
            return txt.Replace("Producer", "Consumer")
                                         .Replace("Publisher", "Subscriber")
                                         .Replace("Pub", "Sub").Trim();
        }
        return "ERROR";
    }

    #endregion // Convert

    #region CopyDocumentation

    /// <summary>
    /// Copies the documentation.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="mds">The MDS.</param>
    /// <param name="indent">The indent.</param>
    public static void CopyDocumentation(StringBuilder source, string kind, CSharpSyntaxNode mds, string indent = "\t\t")
    {
        var trivia = mds.GetLeadingTrivia()
                        .Where(t =>
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                                t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
        foreach (var doc in trivia)
        {
            source.AppendLine($"{indent}/// {Convert(doc.ToString(), kind)}");
        }
    }

    #endregion // CopyDocumentation

    #region // Hierarchy

    //public static IList<INamedTypeSymbol> Hierarchy(this INamedTypeSymbol symbol)
    //{
    //    var hierarchy = new List<INamedTypeSymbol> { symbol };
    //    var s = symbol.BaseType;
    //    while (s != null && s.Name != "Object")
    //    {
    //        hierarchy.Add(s);
    //        s = s.BaseType;
    //    }
    //    return hierarchy;
    //}

    #endregion // Hierarchy

    #region GetVersionInfo

    public static VersionInfo GetVersionInfo(
                                    this AttributeSyntax attribute,
                                    Compilation compilation)
    {
        var attData = attribute.GetAttributesData(
                                    compilation);
        if (attData == null)
            return default;

        var typeRaw = attData.ConstructorArguments.First();
        var type = typeRaw.Value?.Equals(0) ?? true ? EventsContractType.Producer : EventsContractType.Consumer;
        var minVersionRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInfo.MinVersion));
        var minVersion = (int?)minVersionRaw.Value.Value;
        var versionNamingRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInfo.VersionNaming));
        var versionNamingRawValue = (int?)versionNamingRaw.Value.Value;
        var versionNaming = versionNamingRawValue == null ? VersionNaming.Default : (VersionNaming)versionNamingRawValue;
        var ignoreVersionRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInfo.IgnoreVersion));
        var ignoreVersion = ignoreVersionRaw.Value.Kind == TypedConstantKind.Array 
            ? ignoreVersionRaw.Value.Values.Select(m => (int)(m.Value ?? -1)).Where(m => m >= 0).ToArray()
            : Array.Empty<int>();

        var result = new VersionInfo(type) { MinVersion = minVersion ?? 0, VersionNaming = versionNaming, IgnoreVersion = ignoreVersion ?? Array.Empty<int>() };
        return result;
    }

    #endregion // GetVersionInfo

    #region GetOperationVersionInfo

    public static OperationVersionInfo GetOperationVersionInfo(
                                    this MethodDeclarationSyntax method,
                                    Compilation compilation)
    {
        var attData = method.AttributeLists.GetAttributesData(
                                    compilation,
                                    a =>
                                    {
                                        var name = a.Name.ToFullString();
                                        return name.EndsWith("EventSourceVersionAttribute") ||
                                                name.EndsWith("EventSourceVersion");
                                    }).FirstOrDefault();
        if (attData == null)
            return default;

        var versionRaw = attData.ConstructorArguments.First();
        var version = (int)versionRaw.Value!;
        var retiredRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperationVersionInfo.Retired));
        var retired = (int?)retiredRaw.Value.Value;

        var result = new OperationVersionInfo { Version = version, Retired = retired };
        return result;
    }

    #endregion // GetOperationVersionInfo

    #region GetAttributesData

    /// <summary>
    /// Gets the attributes data.
    /// </summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns></returns>
    /// <remarks>
    /// Credits: https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
    /// </remarks>
    public static AttributeData[] GetAttributesData(
                                    this SyntaxList<AttributeListSyntax> attributes,
                                    Compilation compilation,
                                    Predicate<AttributeSyntax>? predicate)
    {
        var list = attributes.SelectMany(m => m.GetAttributesData(compilation, predicate));
        return list.ToArray();
    }

    /// <summary>
    /// Gets the attributes data.
    /// </summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns></returns>
    /// <remarks>
    /// Credits: https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
    /// </remarks>
    public static IEnumerable<AttributeData> GetAttributesData(
                                this AttributeListSyntax attributes,
                                Compilation compilation,
                                Predicate<AttributeSyntax>? predicate)
    {
        // Collect pertinent syntax trees from these attributes
        var acceptedTrees = new HashSet<SyntaxTree>();
        foreach (AttributeSyntax attribute in attributes.Attributes)
        {
            if (predicate?.Invoke(attribute) ?? true)
                acceptedTrees.Add(attribute.SyntaxTree);
        }

        var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var query = from attribute in parentAttributes
                    where acceptedTrees.Contains(attribute.ApplicationSyntaxReference!.SyntaxTree)
                    select attribute;
        return query;
    }


    /// <summary>
    /// Gets the attributes data.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns></returns>
    /// <remarks>
    /// Credits: https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
    /// </remarks>
    public static AttributeData GetAttributesData(
                                this AttributeSyntax attribute,
                                Compilation compilation)
    {
        // Collect pertinent syntax trees from these attributes
        var acceptedTrees = new HashSet<SyntaxTree>();
        acceptedTrees.Add(attribute.SyntaxTree);

        var parentSymbol = attribute.Parent!.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var query = from att in parentAttributes
                    where acceptedTrees.Contains(att.ApplicationSyntaxReference!.SyntaxTree)
                    select att;
        return query.First();
    }

    #endregion // GetAttributesData

    #region GetDeclaredSymbol

    /// <summary>
    /// Gets the declared symbol.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns></returns>
    /// <remarks>
    /// Credits: https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
    /// </remarks>
    public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(node);
        return symbol;
    }

    #endregion // GetDeclaredSymbol
}
