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
        /// <param name="generateFrom">Source of the generation.</param>
        /// <returns>
        /// File name
        /// </returns>
        protected abstract string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom);


        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntax || syntax.TargetAttribute != _targetAttribute) return;

            foreach (var info in syntax.Contracts)
            {
                ExecuteSingle(context, info);
            }
        }

        public void ExecuteSingle(GeneratorExecutionContext context, SyntaxReceiverResult info)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            #region Validation

            if (kind == "NONE")
            {
                context.AddSource($"ERROR.cs", $"// Invalid source input: kind = [{kind}], {item}");
                return;
            }

            #endregion // Validation

            string[] defaultUsing = { "using System.Threading.Tasks;", "using System.CodeDom.Compiler;", "using Weknow.EventSource.Backbone;" };

            var builder = new StringBuilder();
            foreach (var u in defaultUsing)
            {
                builder.AppendLine(u);
            }
            builder.AppendLine();

            var overrideNS = ns;
            if (overrideNS == null && item.Parent is NamespaceDeclarationSyntax ns_)
            {
                foreach (var c in ns_?.Parent?.ChildNodes() ?? Array.Empty<SyntaxNode>())
                {
                    if (c is UsingDirectiveSyntax use)
                    {
                        var u = use.ToFullString().Trim();
                        if (!defaultUsing.Any(m => m == u))
                            builder.AppendLine(u);
                    }
                }
                builder.AppendLine();
                overrideNS = ns_?.Name?.ToString();
            }
            builder.AppendLine($"namespace {overrideNS ?? "Weknow.EventSource.Backbone"}");
            builder.AppendLine("{");
            //CopyDocumentation(builder, kind, item, "\t");

            string generateFrom = item.Identifier.ValueText;
            string interfaceName = name ?? Convert(generateFrom, kind);
            if (!string.IsNullOrEmpty(interfaceName) && !interfaceName.EndsWith(suffix))
                interfaceName = $"{interfaceName}{suffix}";
            string fileName = OnExecute(builder, context, info, interfaceName, generateFrom);
            builder.AppendLine("}");

            context.AddSource($"{fileName}.{kind}.cs", builder.ToString());
        }
    }
}
