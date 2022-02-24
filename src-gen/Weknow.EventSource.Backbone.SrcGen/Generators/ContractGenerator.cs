﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    internal class ContractGenerator : GeneratorBase
    {
        const string TARGET_ATTRIBUTE = "GenerateEventSource";
        private readonly BridgeGenerator _bridge = new BridgeGenerator();

        public ContractGenerator() : base(TARGET_ATTRIBUTE)
        {

        }

        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="info">The information.</param>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <param name="generateFrom"></param>
        /// <returns>
        /// File name
        /// </returns>
        protected override GenInstruction[] OnExecute(
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {

            var (item, att, name, kind, suffix, ns, isProducer) = info;
            var builder = new StringBuilder();
            CopyDocumentation(builder, kind, item, "\t");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            var asm = GetType().Assembly.GetName();
            builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
            builder.AppendLine($"\tpublic interface {interfaceName}");
            builder.AppendLine("\t{");

            var tree = item.SyntaxTree;
            var root = tree.GetRoot();
            var sModel = context.Compilation.GetSemanticModel(tree);
            var classSymbol = sModel.GetDeclaredSymbol(root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First());
            var implementedInterfaces = classSymbol.AllInterfaces;
            foreach (var tp in implementedInterfaces)
            {
                foreach (var method in tp.GetMembers())
                {
                    if (method is IMethodSymbol mds)
                    {
                        //CopyDocumentation(builder, kind, mds);


                        builder.Append("\t\tpublic ValueTask");
                        if (isProducer)
                            builder.Append("<EventKeys>");
                        builder.Append($" {mds.Name}(");

                        var ps = mds.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Name}");
                        builder.Append("\t\t\t");
                        builder.Append(string.Join(", ", ps));
                        builder.AppendLine(");");
                        builder.AppendLine();
                    }
                }
            }
            builder.AppendLine("\t}");

            var contractOnlyArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "ContractOnly");
            var contractOnly = contractOnlyArg?.Expression.NormalizeWhitespace().ToString() == "true";

            info = info.OverrideName(interfaceName);
            if (!contractOnly)
                _bridge.ExecuteSingle(context, info);

            return new[] { new GenInstruction(interfaceName, builder.ToString())};
        }
    }
}
