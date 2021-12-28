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
    /// Syntax Receiver
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.ISyntaxReceiver" />
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public ImmutableArray<SyntaxReceiverResult> Contracts =
            ImmutableArray<SyntaxReceiverResult>.Empty;

        private const string ATTRIBUTE_SUFFIX = "Attribute";

        private readonly ImmutableHashSet<string> _targetAttribute = ImmutableHashSet<string>.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxReceiver"/> class.
        /// </summary>
        /// <param name="targetAttribute">The target attribute.</param>
        public SyntaxReceiver(string targetAttribute)
        {
            TargetAttribute = targetAttribute;
            _targetAttribute = _targetAttribute.Add(targetAttribute);
            if (targetAttribute.EndsWith(ATTRIBUTE_SUFFIX))
                _targetAttribute = _targetAttribute.Add(targetAttribute.Substring(0, targetAttribute.Length - ATTRIBUTE_SUFFIX.Length));
            else
                _targetAttribute = _targetAttribute.Add($"{targetAttribute}{ATTRIBUTE_SUFFIX}");
        }

        /// <summary>
        /// Gets the target attribute.
        /// </summary>
        public string TargetAttribute { get; }

        /// <summary>
        /// Called for each <see cref="T:Microsoft.CodeAnalysis.SyntaxNode" /> in the compilation
        /// </summary>
        /// <param name="syntaxNode">The current <see cref="T:Microsoft.CodeAnalysis.SyntaxNode" /> being visited</param>
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
