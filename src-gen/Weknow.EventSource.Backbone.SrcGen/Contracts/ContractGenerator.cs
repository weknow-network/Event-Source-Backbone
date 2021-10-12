using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    internal class ContractGenerator : GeneratorBase
    {
        const string TARGET_ATTRIBUTE = "GenerateEventSourceContract";

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
        /// <returns>
        /// File name
        /// </returns>
        protected override string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            CopyDocumentation(builder, kind, item, "\t");

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

            return interfaceName;
        }
    }
}
