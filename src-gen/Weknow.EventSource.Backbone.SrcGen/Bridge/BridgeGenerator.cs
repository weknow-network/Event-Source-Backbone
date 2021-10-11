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

                var ctorArg = att.ArgumentList?.Arguments[0].GetText().ToString();
                string kind = ctorArg?.Substring("EventSourceGenType.".Length) ?? "NONE";

                var autoSuffixArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "AutoSuffix");
                var autoSuffix = autoSuffixArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "") == "true" ? kind : string.Empty;

                var contractSyntaxt = att.ArgumentList?.Arguments[1]
                                                       ?.Expression
                                                       .NormalizeWhitespace()?
                                                       .ChildNodes()?
                                                       .FirstOrDefault() as IdentifierNameSyntax;
                if (contractSyntaxt == null) throw new ArgumentNullException("GenerateEventSourceBridge");
                // TODO: [bnaya 2021] for Avi: how can I get the TypeDeclarationSyntax from contractSyntaxt

                //var x = contractSyntaxt.Identifier.ValueText;
                //var i = context.Compilation.GetTypeByMetadataName(contractSyntaxt.Identifier.ToFullString());
                //var x1 = contractSyntaxt.GetType().Name;

                src.AppendLine($"// {kind}, {name}, {autoSuffix}");
            }
            context.AddSource("All.cs", src.ToString());

            //foreach (var (item, name, kind, suffix) in syntax.Contracts)
            //{
            //    #region Validation

            //    if (kind == "NONE")
            //    {
            //        context.AddSource($"ERROR.cs", $"// Invalid source input: kind = [{kind}], {item}");
            //        continue;
            //    }

            //    #endregion // Validation


            //    var source = new StringBuilder();
            //    source.AppendLine($"// {item}");

            //    context.AddSource($"{Convert(item.Name.ToString(), kind)}{suffix}.cs", source.ToString());
            //}
        }
    }
}