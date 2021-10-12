using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    internal class BridgeGenerator : GeneratorBase
    {
        const string TARGET_ATTRIBUTE = "GenerateEventSourceBridge";

        public BridgeGenerator() : base(TARGET_ATTRIBUTE)
        {

        }

        protected override string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            var verrideInterfaceArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "InterfaceName");
            var overrideInterfaceName = verrideInterfaceArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");


            // CopyDocumentation(builder, kind, item, "\t");

            string prefix = interfaceName.StartsWith("I") &&
                interfaceName.Length > 1 &&
                Char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
            string fileName = $"{prefix}Bridge";

            builder.AppendLine($"\tpublic class {fileName}: ProducerPipeline, {overrideInterfaceName ?? interfaceName}");
            builder.AppendLine("\t{");

            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;

                CopyDocumentation(builder, kind, mds);

                string mtdName = mds.Identifier.ValueText;
                builder.Append("\t\tpublic ValueTask");
                if (isProducer)
                    builder.Append("<EventKeys>");
                builder.Append($" {mtdName}(");

                var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                builder.Append("\t\t\t");
                builder.Append(string.Join(", ", ps));
                builder.AppendLine(")");
                builder.AppendLine("\t\t\t{");
                builder.AppendLine($"\t\t\tvar operation = nameof({interfaceName}.{mtdName});");
                foreach (var p in mds.ParameterList.Parameters)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\tvar classification0 = CreateClassificationAdaptor(operation, nameof({pName}), {pName});");

                }
                builder.AppendLine("\t\t\t}");
                builder.AppendLine();

            }
            builder.AppendLine("\t}");

            return fileName;
        }
    }
}