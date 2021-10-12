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
    internal abstract class GeneratorBase : ISourceGenerator
    {
        private readonly string _targetAttribute;

        public GeneratorBase(string targetAttribute)
        {
            _targetAttribute = targetAttribute;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver(_targetAttribute));
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
        protected abstract string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName);


        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntax || syntax.TargetAttribute != _targetAttribute) return;

            foreach (var info in syntax.Contracts)
            {
                var (item, att, name, kind, suffix, ns, isProducer) = info;

                #region Validation

                if (kind == "NONE")
                {
                    context.AddSource($"ERROR.cs", $"// Invalid source input: kind = [{kind}], {item}");
                    continue;
                }

                #endregion // Validation

                var builder = new StringBuilder();
                builder.AppendLine("using System.Threading.Tasks;");
                builder.AppendLine("using System.CodeDom.Compiler;");
                builder.AppendLine("using Weknow.EventSource.Backbone;");
                builder.AppendLine();

                var overrideNS = ns;
                if (overrideNS == null && item.Parent is NamespaceDeclarationSyntax ns_)
                {
                    foreach (var c in ns_?.Parent?.ChildNodes() ?? Array.Empty<SyntaxNode>())
                    {
                        if (c is UsingDirectiveSyntax use)
                            builder.AppendLine(use.ToFullString());
                    }
                    builder.AppendLine();
                    overrideNS = ns_?.Name?.ToString();
                }
                builder.AppendLine($"namespace {overrideNS ?? "Weknow.EventSource.Backbone"}");
                builder.AppendLine("{");
                CopyDocumentation(builder, kind, item, "\t");

                string interfaceName = name ?? $"{Convert(item.Identifier.ValueText, kind)}{suffix}";
                builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
                string fileName = OnExecute(builder, context, info, interfaceName);
                builder.AppendLine("}");

                context.AddSource($"{fileName}.cs", builder.ToString());
            }
        }
    }
}
