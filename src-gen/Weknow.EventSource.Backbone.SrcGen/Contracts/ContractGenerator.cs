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
    public class ContractGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ContractSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not ContractSyntaxReceiver syntax) return;

            foreach (var (item, name, kind, suffix) in syntax.Contracts)
            {
                #region Validation

                if (kind == "NONE")
                {
                    context.AddSource($"ERROR.cs", $"// Invalid source input: kind = [{kind}], {item}");
                    continue;
                }

                #endregion // Validation

                var isProducer = kind == "Producer";

                var source = new StringBuilder();
                source.AppendLine("using System.Threading.Tasks;");
                source.AppendLine("using System.CodeDom.Compiler;");
                source.AppendLine("using Weknow.EventSource.Backbone;");
                source.AppendLine();

                if (item.Parent is NamespaceDeclarationSyntax ns)
                {
                    if (ns.Parent != null)
                    {
                        foreach (var c in ns.Parent.ChildNodes())
                        {
                            if (c is UsingDirectiveSyntax use)
                                source.AppendLine(use.ToFullString());
                        }
                        source.AppendLine();
                    }
                    source.AppendLine($"namespace {ns.Name}");
                    source.AppendLine("{");

                }
                CopyDocumentation(source, item, "\t");
                source.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
                source.AppendLine($"\tpublic interface {name ?? Convert(item.Identifier.ValueText, kind)}{suffix}");
                source.AppendLine("\t{");

                foreach (var method in item.Members)
                {
                    if (method is MethodDeclarationSyntax mds)
                    {
                        CopyDocumentation(source, mds);


                        source.Append("\t\tpublic ValueTask");
                        if (isProducer)
                            source.Append("<EventKeys>");
                        source.Append($" {mds.Identifier.ValueText}(");

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

                context.AddSource($"{Convert(item.Identifier.ValueText, kind)}{suffix}.cs", source.ToString());

                #region CopyDocumentation

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

                #endregion // CopyDocumentation
            }
        }
    }
}
