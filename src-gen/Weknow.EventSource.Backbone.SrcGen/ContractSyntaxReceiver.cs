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
        public ImmutableArray<(TypeDeclarationSyntax type, string? name)> ConsumerContracts = ImmutableArray<(TypeDeclarationSyntax type, string? name)>.Empty;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax tds &&
                    tds.Kind() == SyntaxKind.InterfaceDeclaration)
            {
                var atts = from al in tds.AttributeLists
                           from a in al.Attributes
                           let n = a.Name.ToString()
                           where n == "GenerateEventSourceProducerContractAttribute" ||
                                 n == "GenerateEventSourceProducerContract" 
                           select a;
                var first = atts.FirstOrDefault();
                if (first == null) return;
                var arg = first.ArgumentList?.Arguments.FirstOrDefault();
                // TODO: [bnaya 2021-10] ask Avi
                var val = arg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");
                ConsumerContracts = ConsumerContracts.Add((tds, val));  
            }
        }
    }
}
