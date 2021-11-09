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
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public ImmutableArray<SyntaxReceiverResult> Contracts =
            ImmutableArray<SyntaxReceiverResult>.Empty;
        private const string ATTRIBUTE_SUFFIX = "Attribute";

        private readonly ImmutableHashSet<string> _targetAttribute = ImmutableHashSet<string>.Empty;

        public SyntaxReceiver(string targetAttribute)
        {
            TargetAttribute = targetAttribute;
            _targetAttribute = _targetAttribute.Add(targetAttribute);
            if (targetAttribute.EndsWith(ATTRIBUTE_SUFFIX))
                _targetAttribute = _targetAttribute.Add(targetAttribute.Substring(0, targetAttribute.Length - ATTRIBUTE_SUFFIX.Length));
            else
                _targetAttribute = _targetAttribute.Add($"{targetAttribute}{ATTRIBUTE_SUFFIX}");
        }

        public string TargetAttribute { get; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not TypeDeclarationSyntax tds ||
                    tds.Kind() != SyntaxKind.InterfaceDeclaration)
            {
                return;
            }

            var atts = from al in tds.AttributeLists
                       from a in al.Attributes
                       let n = a.Name.ToString()
                       where _targetAttribute.Contains(n)
                       select a;
            foreach (AttributeSyntax att in atts)
            {
                var nameArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "Name");
                var name = nameArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

                var nsArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "Namespace");
                var ns = nsArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

                var ctorArg = att.ArgumentList?.Arguments.First().GetText().ToString();
                string kind = ctorArg?.Substring("EventSourceGenType.".Length) ?? "NONE";

                var result = new SyntaxReceiverResult(tds, name, ns, kind, att);
                Contracts = Contracts.Add(result);
            }
        }
    }
}
