using System.Text;
using System.Xml.Linq;


using EventSourcing.Backbone.SrcGen.Entities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
    private static readonly Predicate<AttributeData> DEFAULT_VERSION_DEPRECATION_PREDICATE =
                                a =>
                                {
                                    var name = a.AttributeClass!.Name;
                                    return name.EndsWith("EventSourceDeprecateVersionAttribute") ||
                                            name.EndsWith("EventSourceDeprecateVersion");
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

            source.AppendLine($"{indent}/// <summary>");
            source.AppendLine($"{indent}/// Generated documentation");
            source.AppendLine($"{indent}/// </summary>");
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

            if (source.Length == 0)
            {
                source.AppendLine($"{indent}/// <summary>");
                source.AppendLine($"{indent}/// Generated documentation");
                source.AppendLine($"{indent}/// </summary>");
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
                                    this SyntaxReceiverResult info,
                                    Compilation compilation,
                                    string kind)
    {
        return info.Att.GetVersionInfo(compilation, kind);
    }

    public static VersionInstructions GetVersionInfo(
                                    this AttributeSyntax attribute,
                                    Compilation compilation,
                                    string kind)
    {
        var attData = attribute.GetAttributesData(
                                    compilation, kind);

        return attData.GetVersionInfo();

    }

    public static VersionInstructions GetVersionInfo(
                        this AttributeData? attributeData)
    {
        if (attributeData == null)
            return default;

        var minVersionRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.MinVersion));
        var minVersion = (int?)minVersionRaw.Value.Value;
        var versionNamingRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.VersionNaming));
        var versionNamingRawValue = (int?)versionNamingRaw.Value.Value;
        var versionNaming = versionNamingRawValue == null ? VersionNaming.Default : (VersionNaming)versionNamingRawValue;
        var entityConventionRaw = attributeData.NamedArguments.FirstOrDefault(m => m.Key == nameof(VersionInstructions.EntityConvention));
        var entityConventionRawValue = (int?)entityConventionRaw.Value.Value;
        var entityConvention = entityConventionRawValue == null ? EntityConvention.Default : (EntityConvention)entityConventionRawValue;

        var result = new VersionInstructions { MinVersion = minVersion ?? 0, VersionNaming = versionNaming, EntityConvention = entityConvention };
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

    public static OperatioDeprecationInstructions? GetOperationDeprecationInfo(
                                    this IMethodSymbol method,
                                    string type)
    {
        AttributeData? opAtt = method.GetAttributesData(type, DEFAULT_VERSION_DEPRECATION_PREDICATE);
        var result = opAtt.GetOperationDeprecationInfo();
        return result;
    }

    private static OperatioDeprecationInstructions? GetOperationDeprecationInfo(
                                this AttributeData? attData)
    {
        if (attData == null)
            return null;

        var kindRaw = attData.ConstructorArguments[0];
        var kind = CastKind(kindRaw.Value);
        var remarkRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioDeprecationInstructions.Remark));
        var remark = (string?)remarkRaw.Value.Value;
        var dateRaw = attData.NamedArguments.FirstOrDefault(m => m.Key == nameof(OperatioDeprecationInstructions.Date));
        var date = (string?)dateRaw.Value.Value;

        var result = new OperatioDeprecationInstructions(kind, remark, date);
        return result;
    }

    #endregion // GetOperationDeprecationInfo

    #region GetAttributesData

    public static AttributeData? GetAttributesData(
                                    this IMethodSymbol method,
                                    string kind,
                                    Predicate<AttributeData> predicate)
    {
        AttributeData? attData = method.GetAttributes()
                                        .FirstOrDefault(a => a.IsOfKind(kind) && predicate(a));
        return attData;
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
            var val = m.ConstructorArguments[0].Value;

            return IsOfKind(val, kind);
        });
    }

    #endregion // GetAttributesData

    #region IsOfKind

    private static bool IsOfKind(this AttributeData attData, string kind)
    {
        var valueRaw = attData.ConstructorArguments[0];
        var value = valueRaw.Value;
        return IsOfKind(value, kind);
    }

    private static bool IsOfKind(object? value, string kind)
    {
        return value switch
        {
            0 when kind == nameof(EventsContractType.Producer) => true,
            1 when kind == nameof(EventsContractType.Consumer) => true,
            _ => false
        };
    }

    private static bool IsOfKind(int value, string kind)
    {
        return value switch
        {
            0 when kind == nameof(EventsContractType.Producer) => true,
            1 when kind == nameof(EventsContractType.Consumer) => true,
            _ => false
        };
    }

    #endregion // IsOfKind

    #region CastKind

    private static string CastKind(object? value)
    {
        return value switch
        {
            0 => nameof(EventsContractType.Producer),
            1 => nameof(EventsContractType.Consumer),
            _ => throw new NotImplementedException()
        };
    }

    private static string CastKind(int value)
    {
        return value switch
        {
            0 => nameof(EventsContractType.Producer),
            1 => nameof(EventsContractType.Consumer),
            _ => throw new NotImplementedException()
        };
    }

    #endregion // CastKind

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
        VersionInstructions versionInfo = info.GetVersionInfo(compilation, kind);
        var methods = info.Symbol.GetAllMethods();
        MethodBundle[] items = methods.Select(method =>
        {
            OperatioVersionInstructions opVersionInfo = method.GetOperationVersionInfo();
            var version = opVersionInfo.Version;
            var deprecatedInfo = method.GetOperationDeprecationInfo(kind);
            bool deprecated = versionInfo.MinVersion > version || deprecatedInfo != null;
            if (deprecated && !withDeprecated)
            {
                return null;
            }

            string mtdName = method.ToNameConvention();
            string mtdShortName = mtdName.EndsWith("Async")
                        ? mtdName.Substring(0, mtdName.Length - 5)
                        : mtdName;

            string prmSig = method.GetParamsSignature();

            var res = new MethodBundle(method,
                                    mtdShortName,
                                    mtdName,
                                    version,
                                    versionInfo,
                                    prmSig,
                                    deprecated);
            return res;
        })
        .Where(m => m != null)
        .Cast<MethodBundle>()
        .ToArray();
        return items;
    }

    #endregion // ToBundle

    #region GetInterceptorsMethods

    /// <summary>
    /// Gets the interceptors methods.
    /// Useful for version migration.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="interfaceName">Name of the interface.</param>
    /// <returns></returns>
    public static IMethodSymbol[] GetInterceptorsMethods(this ITypeSymbol type, string interfaceName)
    {
        IMethodSymbol[] interceptions = type.GetMembers()
                      .Select(m => m as IMethodSymbol)
                      .Where(m => m != null && m.IsStatic)
                      .Cast<IMethodSymbol>()
                      .Where(m => m.Parameters.Length == 2 &&
                                             m.Parameters[0].Type.Name == "IConsumerInterceptionContext" &&
                                             m.Parameters[1].Type.Name == interfaceName &&
                                             (m.ReturnType.ToString() == "System.Threading.Tasks.Task<bool>" ||
                                              m.ReturnType.ToString() == "System.Threading.Tasks.ValueTask<bool>"))
                      .ToArray();

        return interceptions;
    }

    #endregion // GetInterceptorsMethods

    #region GetInterceptors

    /// <summary>
    /// Gets the interceptors methods names.
    /// Useful for version migration.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="interfaceName">Name of the interface.</param>
    /// <returns></returns>
    public static IEnumerable<string> GetInterceptors(this ITypeSymbol type, string interfaceName)
    {
        var interceptions = type.GetInterceptorsMethods(interfaceName);

        var list = new List<string>();
        foreach (IMethodSymbol interception in interceptions)
        {
            var methodDeclaration = interception.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;

            if (methodDeclaration == null)
            {
                continue;
            }
            list.Add(interception.Name);
        }
        return list;
    }

    #endregion // GetInterceptors

    #region AddInterceptors

    /// <summary>
    /// Useful for version migration.
    /// Adds the interceptors.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="type">The type.</param>
    /// <param name="interfaceName">Name of the interface.</param>
    /// <returns></returns>
    public static IEnumerable<string> AddInterceptors(this StringBuilder builder, ITypeSymbol type, string interfaceName)
    {
        var interceptions = type.GetInterceptorsMethods(interfaceName);

        var list = new List<string>();
        foreach (IMethodSymbol interception in interceptions)
        {
            var methodDeclaration = interception.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;

            if (methodDeclaration == null)
            {
                continue;
            }
            var methodSyntaxRoot = methodDeclaration.SyntaxTree.GetRoot();
            var start = methodDeclaration.Span.Start;
            var end = methodDeclaration.Span.End;
            var methodContent = methodSyntaxRoot.GetText().GetSubText(new TextSpan(start, end - start)).ToString();

            builder.Append("\t\t");
            builder.AppendLine(methodContent);
            builder.AppendLine();
            list.Add(interception.Name);
        }
        return list;
    }

    #endregion // AddInterceptors

    #region ToClassName

    public static string ToClassName(this string interfaceName)
    {
        string result = interfaceName.StartsWith("I") &&
        interfaceName.Length > 1 &&
        char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
        return result;
    }

    #endregion // ToClassName
}