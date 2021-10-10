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
        public ImmutableArray<TypeDeclarationSyntax> ConsumerContracts = ImmutableArray<TypeDeclarationSyntax>.Empty;

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
                if (!atts.Any()) return;

                ConsumerContracts = ConsumerContracts.Add(tds);
            }
        }
    }
}
