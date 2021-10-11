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
    public class BridgeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BridgeSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not BridgeSyntaxReceiver syntax) return;

            var src = new StringBuilder();
            foreach (var att in syntax.Attributes)
            {
                var nameArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "Name");
                var name = nameArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

                var nsArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "Namespace");
                var ns = nsArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

                var ctorArg = att.ArgumentList?.Arguments[0].GetText().ToString();
                string kind = ctorArg?.Substring("EventSourceGenType.".Length) ?? "NONE";

                var autoSuffixArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "AutoSuffix");
                var suffix = autoSuffixArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "") == "true" ? kind : string.Empty;

                var contractSyntaxt = (att.ArgumentList?.Arguments[1]?.Expression as TypeOfExpressionSyntax)?.Type;
                if (contractSyntaxt == null) throw new ArgumentNullException("GenerateEventSourceBridge");

                var interfaceName = contractSyntaxt.Identifier.ValueText;
                var contract = context.Compilation.SyntaxTrees.SelectMany(x =>
                            x.GetRoot()
                            .DescendantNodes()
                            .Where(x => x is TypeDeclarationSyntax tds &&
                                tds.Kind() == SyntaxKind.InterfaceDeclaration &&
                                tds.Identifier.ValueText == interfaceName)
                            .Select(x => x as TypeDeclarationSyntax)).First();


                #region Validation

                if (contract == null) throw new ArgumentNullException("Cannot find the interface");

                #endregion // Validation
                var isProducer = kind == "Producer";

                var source = new StringBuilder();
                source.AppendLine("using System.Threading.Tasks;");
                source.AppendLine("using System.CodeDom.Compiler;");
                source.AppendLine("using Weknow.EventSource.Backbone;");
                source.AppendLine();

                if (ns == null && contract.Parent is NamespaceDeclarationSyntax ns_)
                {
                    if (ns_.Parent != null)
                    {
                        foreach (var c in ns_.Parent.ChildNodes())
                        {
                            if (c is UsingDirectiveSyntax use)
                                source.AppendLine(use.ToFullString());
                        }
                        source.AppendLine();
                        ns = ns_.Name.ToString();
                    }
                }
                string fileName = $"{name ?? Convert(interfaceName.Substring(1), kind)}{suffix}";

                source.AppendLine($"namespace {ns ?? "Weknow.EventSource.Backbone"}");
                source.AppendLine("{");

                //CopyDocumentation(source, kind, item, "\t");
                source.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
                source.AppendLine($"\tpublic class {fileName}: ProducerPipeline, {interfaceName}");
                source.AppendLine("\t{");

                foreach (var method in contract.Members)
                {
                    if (method is not MethodDeclarationSyntax mds)
                        continue;

                    CopyDocumentation(source, kind, mds);

                    string mtdName = mds.Identifier.ValueText;
                    source.Append("\t\tpublic ValueTask");
                    if (isProducer)
                        source.Append("<EventKeys>");
                    source.Append($" {mtdName}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                    source.Append("\t\t\t");
                    source.Append(string.Join(", ", ps));
                    source.AppendLine(")");
                    source.AppendLine("\t\t\t{");
                    source.AppendLine($"\t\t\tvar operation = nameof({interfaceName}.{mtdName});");
                    foreach (var p in mds.ParameterList.Parameters)
                    {
                        var pName = p.Identifier.ValueText;
                        source.AppendLine($"\t\t\tvar classification0 = CreateClassificationAdaptor(operation, nameof({pName}), {pName});");

                    }
                    source.AppendLine("\t\t\t}");
                    source.AppendLine();

                }
                source.AppendLine("\t}");
                source.AppendLine("}");

                context.AddSource($"{fileName}.cs", source.ToString());
            }
        }
    }
}