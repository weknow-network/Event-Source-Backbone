using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Syntax Receiver Result Extentions
    /// </summary>
    internal static class SyntaxReceiverResultExtentions
    {
        /// <summary>
        /// Overrides the name.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static SyntaxReceiverResult OverrideName(this SyntaxReceiverResult source, string name)
        {
            return new SyntaxReceiverResult(source.Type, name, source.Namespace, source.Kind, source.Att);
        }
    }

    /// <summary>
    /// Syntax Receiver Result
    /// </summary>
    internal class SyntaxReceiverResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxReceiverResult"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="att">The attribute.</param>
        public SyntaxReceiverResult(
            TypeDeclarationSyntax type,
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
        /// Deconstruct the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="att">The attribute.</param>
        /// <param name="name">The name.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="isProducer">if set to <c>true</c> [is producer].</param>
        public void Deconstruct(out TypeDeclarationSyntax type, out AttributeSyntax att, out string? name, out string kind, out string suffix, out string? ns, out bool isProducer)
        {
            type = Type;
            att = Att;
            name = Name;
            kind = Kind;
            suffix = Suffix;
            ns = Namespace;
            isProducer = IsProducer;
        }
    }
}
