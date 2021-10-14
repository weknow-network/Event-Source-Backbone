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
    internal static class SyntaxReceiverResultExtentions
    {
        public static SyntaxReceiverResult OverrideName(this SyntaxReceiverResult source, string name)
        {
            return new SyntaxReceiverResult(source.Type, name, source.Namespace, source.Kind, source.Att);
        }
    }

    internal class SyntaxReceiverResult
    {
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

        public TypeDeclarationSyntax Type { get; }
        public string? Name { get; }
        public string Kind { get; }
        public string Suffix => Name == null ? Kind : string.Empty;

        public string? Namespace { get; }

        public bool IsProducer => Kind == "Producer";

        public AttributeSyntax Att { get; }

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
