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
        private readonly string _kindFilter;
        private static readonly string[] DEFAULT_USING = new[]{
                            "System",
                            "System.Linq",
                            "System.Collections",
                            "System.Collections.Generic",
                            "System.Threading.Tasks",
                            "System.CodeDom.Compiler",
                            "Weknow.EventSource.Backbone",
                            "Weknow.EventSource.Backbone.Building" }.Select(u => $"using {u};").ToArray();


        public GeneratorBase(string targetAttribute, KindFilter kindFilter = KindFilter.Any)
        {
            _targetAttribute = targetAttribute;
            _kindFilter = kindFilter.ToString();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver(_targetAttribute));
        }

        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="info">The information.</param>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <param name="generateFrom">Source of the generation.</param>
        /// <returns>
        /// File name
        /// </returns>
        protected abstract GenInstruction[] OnExecute(
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom);


        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver syntax || syntax.TargetAttribute != _targetAttribute) return;

            foreach (var info in syntax.Contracts)
            {
                if(_kindFilter == nameof(KindFilter.Any) || info.Kind == _kindFilter)
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

            string generateFrom = item.Identifier.ValueText;
            string interfaceName = GetInterfaceConvention(name, generateFrom, kind, suffix);

            GenInstruction[] codes = OnExecute(context, info, interfaceName, generateFrom);

            foreach (var (fileName, content, dynamicNs, usn) in codes)
            {
                var builder = new StringBuilder();
                var usingSet = new HashSet<string>(DEFAULT_USING);

                builder.AppendLine();
                builder.AppendLine("#nullable enable");

                foreach (var u in usn)
                {
                    if(!usingSet.Contains(u))
                        usingSet.Add(u);
                }

                var overrideNS = dynamicNs ?? ns;
                if (overrideNS == null && item.Parent is NamespaceDeclarationSyntax ns_)
                {
                    foreach (var c in ns_?.Parent?.ChildNodes() ?? Array.Empty<SyntaxNode>())
                    {
                        if (c is UsingDirectiveSyntax use)
                        {
                            var u = use.ToFullString().Trim();
                            if (!usingSet.Contains(u))
                                usingSet.Add(u);
                        }
                    }
                    builder.AppendLine();
                    overrideNS = ns_?.Name?.ToString();
                }
                foreach (var u in usingSet.OrderBy(m => m))
                {
                    builder.AppendLine(u);
                }
                builder.AppendLine($"namespace {overrideNS ?? "Weknow.EventSource.Backbone"}");
                builder.AppendLine("{");
                builder.AppendLine(content);
                builder.AppendLine("}");

                context.AddSource($"{fileName}.{kind}.cs", builder.ToString());
            }
        }

        protected virtual string GetInterfaceConvention(string? name, string generateFrom, string kind, string? suffix)
        {
            string interfaceName = name ?? Convert(generateFrom, kind);
            if (name == null && !string.IsNullOrEmpty(interfaceName) && !interfaceName.EndsWith(suffix))
                interfaceName = $"{interfaceName}{suffix}";
            return interfaceName;
        }
    }
}
