using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Weknow.EventSource.Backbone;

///// <summary>
///// Syntax Receiver Result Extensions
///// </summary>
//internal static class SyntaxReceiverResultExtentions
//{
//    /// <summary>
//    /// Overrides the name.
//    /// </summary>
//    /// <param name="source">The source.</param>
//    /// <param name="name">The name.</param>
//    /// <returns></returns>
//    public static SyntaxReceiverResult OverrideName(this SyntaxReceiverResult source, string name)
//    {
//        return new SyntaxReceiverResult(source.Type, source.Symbol, name, source.Namespace, source.Kind, source.Att);
//    }
//}

/// <summary>
/// Syntax Receiver Result
/// </summary>
internal class SyntaxReceiverResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxReceiverResult" /> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="symbol">The symbol.</param>
    /// <param name="name">The name.</param>
    /// <param name="ns">The namespace.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="att">The attribute.</param>
    /// <exception cref="System.NullReferenceException">symbol</exception>
    public SyntaxReceiverResult(
        TypeDeclarationSyntax type,
        INamedTypeSymbol? symbol,
        string? name,
        string? ns,
        string kind,
        AttributeSyntax att)
    {
        Type = type;
        Kind = kind;
        Name = name;
        Namespace = ns;
        Att = att;
        Symbol = symbol ?? throw new NullReferenceException(nameof(symbol));
        GenerateFrom = type.Identifier.ValueText;
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public TypeDeclarationSyntax Type { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string GenerateFrom { get; }

    /// <summary>
    /// Gets the kind.
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Gets the suffix.
    /// </summary>
    public string Suffix => Name == null ? Kind : string.Empty;

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is producer.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is producer; otherwise, <c>false</c>.
    /// </value>
    public bool IsProducer => Kind == "Producer";

    /// <summary>
    /// Gets the attribute.
    /// </summary>
    public AttributeSyntax Att { get; }

    /// <summary>
    /// Gets the symbol.
    /// </summary>
    public INamedTypeSymbol Symbol { get; }

    #region FormatName

    public string FormatName()
    {
        return FormatName(GenerateFrom);
    }

    public string FormatName(string generateFrom)
    {
        string interfaceName = Name ?? Helper.Convert(generateFrom, Kind);
        if (Name == null && !string.IsNullOrEmpty(interfaceName) && !interfaceName.EndsWith(Suffix))
            interfaceName = $"{interfaceName}{Suffix}";
        return interfaceName;

    }

    #endregion // FormatName

    #region Deconstruct

    /// <summary>
    /// Deconstruct the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="att">The attribute.</param>
    /// <param name="name">The name.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="suffix">The suffix.</param>
    /// <param name="ns">The namespace.</param>
    /// <param name="isProducer">if set to <c>true</c> [is producer].</param>
    public void Deconstruct(out TypeDeclarationSyntax type,
                            out AttributeSyntax att,
                            out INamedTypeSymbol symbol,
                            out string kind,
                            out string? ns,
                            out bool isProducer)
    {
        type = Type;
        symbol = Symbol;
        att = Att;
        kind = Kind;
        ns = Namespace;
        isProducer = IsProducer;
    }

    /// <summary>
    /// Deconstruct the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="att">The attribute.</param>
    /// <param name="name">The name.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="suffix">The suffix.</param>
    /// <param name="ns">The namespace.</param>
    /// <param name="isProducer">if set to <c>true</c> [is producer].</param>
    public void Deconstruct(out TypeDeclarationSyntax type,
                            out AttributeSyntax att,
                            out INamedTypeSymbol symbol,
                            out string? name,
                            out string kind,
                            out string suffix,
                            out string? ns,
                            out bool isProducer)
    {
        type = Type;
        symbol = Symbol;
        att = Att;
        name = Name;
        kind = Kind;
        suffix = Suffix;
        ns = Namespace;
        isProducer = IsProducer;
    }

    #endregion // Deconstruct
}
