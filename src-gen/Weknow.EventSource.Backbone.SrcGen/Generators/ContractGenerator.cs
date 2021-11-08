using Microsoft.CodeAnalysis;
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
        /// <param name="builder">The source builder.</param>
        /// <param name="context">The context.</param>
        /// <param name="info">The information.</param>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <param name="generateFrom"></param>
        /// <returns>
        /// File name
        /// </returns>
        protected override string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {

            var (item, att, name, kind, suffix, ns, isProducer) = info;

            CopyDocumentation(builder, kind, item, "\t");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
            builder.AppendLine($"\tpublic interface {interfaceName}");
            builder.AppendLine("\t{");

            foreach (var method in item.Members)
            {
                if (method is MethodDeclarationSyntax mds)
                {
                    CopyDocumentation(builder, kind, mds);


                    builder.Append("\t\tpublic ValueTask");
                    if (isProducer)
                        builder.Append("<EventKeys>");
                    builder.Append($" {mds.Identifier.ValueText}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                    builder.Append("\t\t\t");
                    builder.Append(string.Join(", ", ps));
                    builder.AppendLine(");");
                    builder.AppendLine();
                }
            }
            builder.AppendLine("\t}");

            var contractOnlyArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "ContractOnly");
            var contractOnly = contractOnlyArg?.Expression.NormalizeWhitespace().ToString() == "true";

            info = info.OverrideName(interfaceName);
            if (!contractOnly)
                _bridge.ExecuteSingle(context, info);

            return interfaceName;
        }

        protected override string GetInterfaceConvention(string? name, string generateFrom, string kind, string? suffix)
        {
            string interfaceName = name ?? Convert(generateFrom, kind);
            if (name == null && !string.IsNullOrEmpty(interfaceName) && !interfaceName.EndsWith(suffix))
                interfaceName = $"{interfaceName}{suffix}";
            return interfaceName;
        }
    }
}
