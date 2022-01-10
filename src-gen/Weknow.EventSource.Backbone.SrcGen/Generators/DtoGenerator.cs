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
    internal class DtoGenerator : GeneratorBase
    {
        const string TARGET_ATTRIBUTE = "GenerateEventSource";
        private readonly BridgeGenerator _bridge = new BridgeGenerator();

        public DtoGenerator() : base(TARGET_ATTRIBUTE, KindFilter.Consumer)
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

            var results = new List<GenInstruction>();
            foreach (var method in item.Members)
            {
                if (method is MethodDeclarationSyntax mds)
                {
                    var builder = new StringBuilder();
                    CopyDocumentation(builder, kind, mds, "\t");

                    string fileName = interfaceName;
                    if (fileName.StartsWith("I") && fileName.Length > 1 && Char.IsUpper(fileName[1]))
                    {
                        fileName = fileName.Substring(1);
                    }
                    if(fileName.EndsWith(nameof(KindFilter.Consumer)))
                        fileName = fileName.Substring(0, fileName.Length - nameof(KindFilter.Consumer).Length);

                    string mtdName = mds.Identifier.ValueText;
                    if(mtdName.EndsWith("Async"))
                        mtdName = mtdName.Substring(0, mtdName.Length - 5);
                    var asm = GetType().Assembly.GetName();
                    builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
                    builder.Append("\tpublic record");
                    builder.Append($" {fileName}_{mtdName}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                    builder.Append("\t\t");
                    builder.Append(string.Join(", ", ps));
                    builder.AppendLine(");");
                    builder.AppendLine();

                    results.Add(new GenInstruction($"{fileName}.{mtdName}.dto", builder.ToString()));
                }
            }


            return results.ToArray();
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
