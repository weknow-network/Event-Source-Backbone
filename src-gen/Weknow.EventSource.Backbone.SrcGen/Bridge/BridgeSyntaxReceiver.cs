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

    internal class BridgeSyntaxReceiver : ISyntaxReceiver
    {
        public ImmutableArray<AttributeSyntax> Attributes =
            ImmutableArray<AttributeSyntax>.Empty;


        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Kind() != SyntaxKind.Attribute || syntaxNode is not AttributeSyntax att)
                return;

            string name = att.Name.ToString();
            if (name != "GenerateEventSourceBridge" && name != "GenerateEventSourceBridgeAttribute")
                return;

            Attributes = Attributes.Add(att);
        }
    }
}
