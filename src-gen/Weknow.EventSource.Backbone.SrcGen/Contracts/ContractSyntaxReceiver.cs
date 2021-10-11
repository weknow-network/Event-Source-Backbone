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

    internal class ContractSyntaxReceiver : ISyntaxReceiver
    {
        public ImmutableArray<(TypeDeclarationSyntax type, string? name, string kind, string suffix)> Contracts =
            ImmutableArray<(TypeDeclarationSyntax type, string? name, string kind, string suffix)>.Empty;

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
                       where n == "GenerateEventSourceContractAttribute" ||
                             n == "GenerateEventSourceContract"
                       select a;
            foreach (var att in atts)
            {

                // TODO: [bnaya 2021-10] ask Avi
                var nameArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "Name");
                var name = nameArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

                var ctorArg = att.ArgumentList?.Arguments.First().GetText().ToString();
                string kind = ctorArg?.Substring("EventSourceGenType.".Length) ?? "NONE";

                var autoSuffixArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "AutoSuffix");
                var autoSuffix = autoSuffixArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "") == "true" ? kind : string.Empty;
                Contracts = Contracts.Add((tds, name, kind, autoSuffix));
            }
        }
    }
}
