using System.Collections.Immutable;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

using EventSourcing.Backbone.SrcGen.Entities;
using EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventSourcing.Backbone;

/// </summary>
internal static class Helper
{
    private static readonly Predicate<AttributeData> DEFAULT_VERSION_PREDICATE =
                                a =>
                                {
                                    var name = a.AttributeClass!.Name;
                                    return name.EndsWith("EventSourceVersionAttribute") ||
                                            name.EndsWith("EventSourceVersion");
                                };

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
    /// <param name="versionInfo">The version information.</param>
    /// <param name="indent">The indent.</param>
    public static void CopyDocumentation(
        StringBuilder source,
        string kind,
        CSharpSyntaxNode mds,
        string indent = "\t\t")
    {
    }

    /// <summary>
    /// Copies the documentation.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="mds">The MDS.</param>
    /// <param name="versionInfo">The version information.</param>
    /// <param name="indent">The indent.</param>
    public static void CopyDocumentation(
        Compilation compilation,
        StringBuilder source,
        string kind,
        CSharpSyntaxNode mds,
        int? version,
        string indent = "\t\t")
    {
        var trivia = mds.GetLeadingTrivia()
                        .Where(t =>
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                                t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
        StringBuilder local = version == null ? source : new StringBuilder();
        foreach (var doc in trivia)
        {
            local.AppendLine($"{indent}/// {Convert(doc.ToString(), kind)}");
        }

        if (mds is not MethodDeclarationSyntax mtd)
            return;

        string prmSig = mtd.GetParamsSignature(compilation);
        if (version == null && prmSig == null)
            return;

        string content = local.ToString();

        int at = content.IndexOf("</remarks>");
        string remark = $@"{indent}/// Event Version {version}
{indent}/// Parameter Signature:  {prmSig}";
        if (at == -1)
        {
            local.AppendLine($"{indent}/// <remarks>");
            local.AppendLine(remark);
            local.AppendLine($"{indent}/// </remarks>");
        }
        else
        {
            local.Insert(at, $"\r\n{remark} ");
        }

        source.AppendLine(local.ToString());
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

    public static VersionInstructions GetVersionInfo(
                                    this AttributeSyntax attribute,
                                    Compilation compilation,
                                    string kind)
    {
        var attData = attribute.GetAttributesData(
                                    compilation, kind);
        if (attData == null)
            return default;

        var typeRaw = attData.ConstructorArguments.First();
        var minVersionRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.MinVersion));
        var minVersion = (int?)minVersionRaw.Value.Value;
        var versionNamingRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.VersionNaming));
        var versionNamingRawValue = (int?)versionNamingRaw.Value.Value;
        var versionNaming = versionNamingRawValue == null ? VersionNaming.Default : (VersionNaming)versionNamingRawValue;
        var ignoreVersionRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.IgnoreVersion));

        IImmutableSet<int> ignoreVersion = ignoreVersionRaw.Value.Kind == TypedConstantKind.Array
            ? ignoreVersionRaw.Value.Values.Select(m => (int)(m.Value ?? -1)).Where(m => m >= 0).ToImmutableHashSet()
            : ImmutableHashSet<int>.Empty;

        EventsContractType knd = kind == nameof(EventsContractType.Producer) ? EventsContractType.Producer : EventsContractType.Consumer;
        var result = new VersionInstructions(knd) { MinVersion = minVersion ?? 0, VersionNaming = versionNaming, IgnoreVersion = ignoreVersion };
        return result;
    }

    #endregion // GetVersionInfo

    #region GetOperationVersionInfo

    public static OperatioVersionInstructions GetOperationVersionInfo(
                                    this MethodDeclarationSyntax method,
                                    Compilation compilation)
    {
        var query = method.AttributeLists.GetAttributesData(
                                    compilation,
                                    a =>
                                    {
                                        var name = a.AttributeClass?.Name ?? string.Empty;
                                        return name.EndsWith("EventSourceVersionAttribute") ||
                                                name.EndsWith("EventSourceVersion");
                                    });
        var attData = query.FirstOrDefault();
        return GetOperationVersionInfo(attData);
    }

    public static OperatioVersionInstructions GetOperationVersionInfo(this IMethodSymbol method,
                                    Predicate<AttributeData>? predicate = null)
    {
        predicate = predicate ?? DEFAULT_VERSION_PREDICATE;
        AttributeData? opAtt = method.GetAttributes().Where(
                            a => predicate?.Invoke(a) ?? true).FirstOrDefault();

        var result = opAtt.GetOperationVersionInfo();
        return result;
    }

    private static OperatioVersionInstructions GetOperationVersionInfo(this AttributeData? attData)
    {
        if (attData == null)
            return default;

        var versionRaw = attData.ConstructorArguments.First();
        var version = (int)versionRaw.Value!;
        var remarkRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioVersionInstructions.Remark));
        var remark = (string?)remarkRaw.Value.Value;
        var dateRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioVersionInstructions.Date));
        var date = (string?)dateRaw.Value.Value;

        var result = new OperatioVersionInstructions { Version = version, Remark = remark, Date = date };
        return result;
    }

    #endregion // GetOperationVersionInfo

    #region GetOperationDeprecationInfo

#pragma warning disable HAA0102 // Non-overridden virtual method call on value type
    public static OperatioDeprecationInstructions? GetOperationDeprecationInfo(
                                    this MethodDeclarationSyntax method,
                                    string? type,
                                    Compilation compilation)
    {
        var query = method.AttributeLists.GetAttributesData(
                                    compilation,
                                    a =>
                                    {
                                        var name = a.AttributeClass?.Name ?? string.Empty;
                                        var result = name.EndsWith("EventSourceDeprecationAttribute") ||
                                                name.EndsWith("EventSourceDeprecation");
                                        if (!result)
                                            return false;
                                        result = a.ConstructorArguments.First().Value == type;
                                        return result;
                                    });
        var attData = query.FirstOrDefault();
        return GetOperationDeprecationInfo(attData);
    }
#pragma warning restore HAA0102 

    public static OperatioDeprecationInstructions? GetOperationDeprecationInfo(this IMethodSymbol method,
                                    EventsContractType type,
                                    Predicate<AttributeData>? predicate = null)
    {
        predicate = predicate ?? DEFAULT_VERSION_PREDICATE;
        AttributeData? opAtt = method.GetAttributes().Where(
                            a =>
                                        predicate?.Invoke(a) ?? true).FirstOrDefault();

        var result = opAtt.GetOperationDeprecationInfo();
        return result;
    }

    private static OperatioDeprecationInstructions? GetOperationDeprecationInfo(this AttributeData? attData)
    {
        if (attData == null)
            return null;

        var versionRaw = attData.ConstructorArguments.First();
        var version = (int)versionRaw.Value!;
        var remarkRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioDeprecationInstructions.Remark));
        var remark = (string?)remarkRaw.Value.Value;
        var dateRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioDeprecationInstructions.Date));
        var date = (string?)dateRaw.Value.Value;

        var result = new OperatioDeprecationInstructions { Version = version, Remark = remark, Date = date };
        return result;
    }

    #endregion // GetOperationDeprecationInfo

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
                                    Predicate<AttributeData>? predicate)
    {
        var list = attributes.SelectMany(m => m.GetAttributesData(compilation, predicate));
        return list.ToArray();
    }

    ///// <summary>
    ///// Gets the attributes data.
    ///// </summary>
    ///// <param name="attributes">The attributes.</param>
    ///// <param name="compilation">The compilation.</param>
    ///// <returns></returns>
    ///// <remarks>
    ///// Credits: https://stackoverflow.com/questions/28947456/how-do-i-get-attributedata-from-attributesyntax-in-roslyn
    ///// </remarks>
    //public static IEnumerable<AttributeData> GetAttributesData(
    //                            this AttributeListSyntax attributes,
    //                            Compilation compilation,
    //                            Predicate<AttributeSyntax>? predicate)
    //{
    //    // Collect pertinent syntax trees from these attributes
    //    var acceptedTrees = new HashSet<SyntaxTree>();
    //    foreach (AttributeSyntax attribute in attributes.Attributes)
    //    {
    //        if (predicate?.Invoke(attribute) ?? true)
    //            acceptedTrees.Add(attribute.SyntaxTree);
    //    }

    //    var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation)!;
    //    var parentAttributes = parentSymbol.GetAttributes();
    //    var query = from attribute in parentAttributes
    //                where acceptedTrees.Contains(attribute.ApplicationSyntaxReference!.SyntaxTree)
    //                select attribute;
    //    return query.Where(attribute => predicate?.Invoke(attribute) ?? true);
    //}

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
                                Predicate<AttributeData>? predicate)
    {
        var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var query = parentAttributes.Where(attribute => predicate?.Invoke(attribute) ?? true);
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
                                Compilation compilation,
                                string kind)
    {
        // Collect pertinent syntax trees from these attributes
        var acceptedTrees = new HashSet<SyntaxTree>();
        acceptedTrees.Add(attribute.SyntaxTree);

        var parentSymbol = attribute.Parent!.Parent!.GetDeclaredSymbol(compilation)!;
        var parentAttributes = parentSymbol.GetAttributes();
        var query = from att in parentAttributes
                    where acceptedTrees.Contains(att.ApplicationSyntaxReference!.SyntaxTree)
                    select att;
        return query.First(m =>
        {
            var val = m.ConstructorArguments.FirstOrDefault().Value;

            return val switch
            {
                0 when kind == nameof(EventsContractType.Producer) => true,
                1 when kind == nameof(EventsContractType.Consumer) => true,
                _ => false
            };
        });
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

    public static string GetParamsSignature(this MemberDeclarationSyntax method, Compilation compilation)
    {
        ISymbol? mtdSymbol = method.GetDeclaredSymbol(compilation);
        var result = mtdSymbol.GetParamsSignature();
        return result;
    }
    public static string GetParamsSignature(this ISymbol? method)
    {
        if (method is not IMethodSymbol mtdSymbol)
            return string.Empty;
        var result = mtdSymbol.Parameters.Select(p => p.Type.Name);
        if (result == null)
            return string.Empty;
        return string.Join(",", result);
    }

    public static MethodBundle[] ToBundle(
                            this SyntaxReceiverResult info,
                            Compilation compilation) 
    {
        TypeDeclarationSyntax item = info.Type;
        var kind = info.Kind;
        var versionInfo = info.Att.GetVersionInfo(compilation, kind);
        MethodBundle[] items = item.Members.Select(method =>
        {
            if (method is not MethodDeclarationSyntax mds)
                return null;
            var opVersionInfo = mds.GetOperationVersionInfo(compilation);
            var version = opVersionInfo.Version;
            if (versionInfo.MinVersion > version || versionInfo.IgnoreVersion.Contains(version))
                return null;

            string mtdName = mds.ToNameConvention();
            string mtdShortName = mtdName.EndsWith("Async")
                        ? mtdName.Substring(0, mtdName.Length - 5)
                        : mtdName;

            string prmSig = method.GetParamsSignature(compilation);

            return new MethodBundle(mds, mtdShortName, version, prmSig);
        })
        .Cast<MethodBundle>()
        .Where(m => m != null)
        .Where(m =>
        {
            MethodDeclarationSyntax mds = m.Method;

            var deprecated = mds.GetOperationDeprecationInfo(kind, compilation);
            return deprecated == null;
        })
        .ToArray();
        return items;
    }
}