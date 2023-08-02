using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;


using EventSourcing.Backbone.SrcGen.Entities;
using EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventSourcing.Backbone;

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
        this ISymbol symbol,
        StringBuilder source,
        int? version,
        string indent = "\t\t")
    {
        if (symbol is not IMethodSymbol)
            return;

        var xmlRaw = symbol.GetDocumentationCommentXml();

        string prmSig = symbol.GetParamsSignature();
        if (version == null && prmSig == null)
        {
            if (xmlRaw != null)
                source.AddpendComment(xmlRaw, indent);
            return;
        }

        string remark = prmSig == null
                            ? $"{indent}/// Event Version {version}"
                            : $@"{indent}/// Event Version {version}
{indent}/// Parameter Signature:  {prmSig}";
        if (string.IsNullOrEmpty(xmlRaw))
        {
            source.AppendLine($"{indent}/// <remarks>");
            source.AppendLine(remark);
            source.AppendLine($"{indent}/// </remarks>");
        }
        else
        {
            var xml = XDocument.Parse(xmlRaw);
            foreach (var comment in xml.Root.Elements().Where(m => m.Name != "remarks"))
            {
                source.AddpendComment(comment, indent);
            }
            var remarkXml = xml.Root.Element("remarks");
            if (remarkXml != null)
            {
                source.AppendLine($"{indent}/// <remarks>");
                source.AddpendComment(remarkXml.Value, indent);
                source.AppendLine($"{indent}/// </remarks>");
            }
        }
    }

    #endregion // CopyDocumentation

    #region AddpendComment

    private static void AddpendComment(this StringBuilder builder, XElement comment, string indent)
    {
        AddpendComment(builder, comment.ToString(), indent);
    }

    private static void AddpendComment(this StringBuilder builder, string comment, string indent)
    {
        using var reader = new StringReader(comment.Trim());
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null)
                break;
            builder.AppendLine($"{indent}/// {line}");
        }
    }

    #endregion // AddpendComment

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

        return attData.GetVersionInfo(kind);

    }

    public static VersionInstructions GetVersionInfo(this IMethodSymbol method, string kind)
    {
        var predicate = DEFAULT_VERSION_PREDICATE;
        AttributeData? attData = method.GetAttributes().Where(
                            a => predicate(a)).FirstOrDefault();

        return attData.GetVersionInfo(kind);
    }

    public static VersionInstructions GetVersionInfo(this AttributeData? attributeData, string kind)
    {
        if (attributeData == null)
            return default;

        var minVersionRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.MinVersion));
        var minVersion = (int?)minVersionRaw.Value.Value;
        var versionNamingRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.VersionNaming));
        var versionNamingRawValue = (int?)versionNamingRaw.Value.Value;
        var versionNaming = versionNamingRawValue == null ? VersionNaming.Default : (VersionNaming)versionNamingRawValue;
        var ignoreVersionRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.IgnoreVersion));

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
                                        result = a.ConstructorArguments.First().Value?.ToString() == type;
                                        return result;
                                    });
        var result = query.Select(attData => attData.GetOperationDeprecationInfo(type))
                            .FirstOrDefault(m => m != null);
        return result;
    }
#pragma warning restore HAA0102 

    public static OperatioDeprecationInstructions? GetOperationDeprecationInfo(this IMethodSymbol method,
                                    string type,
                                    Predicate<AttributeData>? predicate = null)
    {
        predicate = predicate ?? DEFAULT_VERSION_PREDICATE;
        AttributeData? opAtt = method.GetAttributes().Where(
                            a =>
                                        predicate?.Invoke(a) ?? true).FirstOrDefault();

        var result = opAtt.GetOperationDeprecationInfo(type);
        return result;
    }

    private static OperatioDeprecationInstructions? GetOperationDeprecationInfo(this AttributeData? attData, string? type)
    {
        if (attData == null)
            return null;

        var kindRaw = attData.ConstructorArguments[0];
        var kind = kindRaw.Value!.ToString();
        if (kind != type)
            return null;

        var versionRaw = attData.ConstructorArguments[1];
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
        if (string.IsNullOrEmpty(kind))
            return query.First();
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

    #region GetParamsSignature

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

    #endregion // GetParamsSignature

    #region ToBundle

    public static MethodBundle[] ToBundle(
                            this SyntaxReceiverResult info,
                            Compilation compilation,
                            bool withDeprecated = false)
    {
        var kind = info.Kind;
        VersionInstructions versionInfo = info.Att.GetVersionInfo(compilation, kind);
        var methods = info.Symbol.GetAllMethods();
        MethodBundle[] items = methods.Select(method =>
        {
            OperatioVersionInstructions opVersionInfo = method.GetOperationVersionInfo();
            var version = opVersionInfo.Version;
            bool excluded = versionInfo.MinVersion > version || versionInfo.IgnoreVersion.Contains(version);
            if (excluded && !withDeprecated)
            {
                return null;
            }

            string mtdName = method.ToNameConvention();
            string mtdShortName = mtdName.EndsWith("Async")
                        ? mtdName.Substring(0, mtdName.Length - 5)
                        : mtdName;

            string prmSig = method.GetParamsSignature();

            return new MethodBundle(method, mtdShortName, mtdName, version, versionInfo.VersionNaming, prmSig, excluded);
        })
        .Cast<MethodBundle>()
        .Where(m => m != null)
        .Where(m =>
        {
            var deprecated = m.Method.GetOperationDeprecationInfo(kind);
            return deprecated == null;
        })
        .ToArray();
        return items;
    }

    #endregion // ToBundle

    public static string ToClassName(this string interfaceName)
    {
        string result = interfaceName.StartsWith("I") &&
        interfaceName.Length > 1 &&
        char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
        return result;
    }
}