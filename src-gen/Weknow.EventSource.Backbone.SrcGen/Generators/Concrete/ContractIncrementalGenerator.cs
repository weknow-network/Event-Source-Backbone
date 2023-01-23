using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Weknow.EventSource.Backbone.SrcGen.Generators.Entities;

using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    internal class ContractncrementalGenerator : GeneratorIncrementalBase
    {
        private const string TARGET_ATTRIBUTE = "GenerateEventSource";
        private readonly BridgeIncrementalGenerator _bridge = new();

        public ContractncrementalGenerator() : base(TARGET_ATTRIBUTE)
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
        protected override GenInstruction[] OnGenerate(
                            SourceProductionContext context,
                            Compilation compilation,
                            SyntaxReceiverResult info)
        {

            var (type, att, symbol,  kind, ns, isProducer) = info;
            string interfaceName = info.FormatName();
            var builder = new StringBuilder();
            CopyDocumentation(builder, kind, type, "\t");
            var asm = GetType().Assembly.GetName();
            builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
            builder.Append($"\tpublic interface {interfaceName}");
            var baseTypes = symbol.Interfaces.Select(m => info.FormatName(m.Name));
            string inheritance = string.Join(", ", baseTypes);
            if (string.IsNullOrEmpty(inheritance))
                builder.AppendLine();
            else
                builder.AppendLine($" : {inheritance}");
            builder.AppendLine("\t{");

            foreach (var method in type.Members)
            {
                if (method is MethodDeclarationSyntax mds)
                {
                    CopyDocumentation(builder, kind, mds);


                    builder.Append("\t\tValueTask");
                    if (isProducer)
                        builder.Append("<EventKeys>");
                    builder.Append($" {mds.Identifier.ValueText}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"\r\n\t\t\t{p.Type} {p.Identifier.ValueText}");
                    builder.Append("\t\t\t");
                    builder.Append(string.Join(", ", ps));
                    builder.AppendLine(");");
                    builder.AppendLine();
                }
            }
            builder.AppendLine("\t}");

            var contractOnlyArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "ContractOnly");
            var contractOnly = contractOnlyArg?.Expression.NormalizeWhitespace().ToString() == "true";

            //info = info.OverrideName(interfaceName);
            if (!contractOnly)
                _bridge.GenerateSingle(context, compilation, info);

            return new[] { new GenInstruction(interfaceName, builder.ToString()) };
        }
    }
}
