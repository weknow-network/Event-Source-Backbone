using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    public class GenContract : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ContractSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not ContractSyntaxReceiver syntax) return;

            foreach (var (item, name, kind) in syntax.ConsumerContracts)
            {
                if (kind == "NONE")
                {
                    context.AddSource($"ERROR.cs", $"// Invalid source input: kind = [{kind}], {item}");
                    continue;
                }

                var source = new StringBuilder();
                source.AppendLine("using System.Threading.Tasks;");
                source.AppendLine("using System.CodeDom.Compiler;");
                source.AppendLine();

                if (item.Parent is NamespaceDeclarationSyntax ns)
                {
                    if (ns.Parent != null)
                    {
                        foreach (var c in ns.Parent.ChildNodes())
                        {
                            if (c is UsingDirectiveSyntax use) source.AppendLine(use.ToFullString());
                        }
                        source.AppendLine();
                    }
                    source.AppendLine($"namespace {ns.Name}");
                    source.AppendLine("{");

                }
                CopyDocumentation(source, item, "\t");
                source.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
                source.AppendLine($"\tpublic interface {name ?? Convert(item.Identifier.ValueText, kind)}");
                source.AppendLine("\t{");

                foreach (var method in item.Members)
                {
                    if (method is MethodDeclarationSyntax mds)
                    {
                        CopyDocumentation(source, mds);

                        source.Append($"\t\tpublic ValueTask {mds.Identifier.ValueText}(");

                        var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                        source.Append("\t\t\t");
                        source.Append(string.Join(", ", ps));
                        source.AppendLine(");");
                        source.AppendLine();
                    }
                }
                source.AppendLine("\t}");
                if (item.Parent is NamespaceDeclarationSyntax)
                {
                    source.AppendLine("}");
                }

                context.AddSource($"{Convert(item.Identifier.ValueText, kind)}.cs", source.ToString());

                string Convert(string txt, string kind)
                {
                    if (kind == "EventSourceGenType.Producer")
                    {
                        return txt.Replace("Consumer", "Producer")
                                                     .Replace("Publisher", "Subscriber")
                                                     .Replace("Pub", "Sub");
                    }
                    if (kind == "EventSourceGenType.Consumer")
                    {
                        return txt.Replace("Producer", "Consumer")
                                                     .Replace("Subscriber", "Publisher")
                                                     .Replace("Sub", "Pub");
                    }
                    return "ERROR";
                }

                void CopyDocumentation(StringBuilder source, CSharpSyntaxNode mds, string indent = "\t\t")
                {
                    var trivia = mds.GetLeadingTrivia()
                                    .Where(t =>
                                            t.Kind() == SyntaxKind.MultiLineDocumentationCommentTrivia ||
                                            t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);
                    foreach (var doc in trivia)
                    {
                        source.AppendLine($"{indent}/// {Convert(doc.ToString(), kind)}");
                    }
                }
            }
        }
    }
}
