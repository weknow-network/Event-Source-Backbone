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

        #region OnExecute

        protected override string OnExecute(
            StringBuilder builder,
            GeneratorExecutionContext context,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            var verrideInterfaceArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "InterfaceName");
            var overrideInterfaceName = verrideInterfaceArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

            if (info.Kind == "Producer")
            {
                return OnExecuteProducer(builder, info, interfaceName, generateFrom);
            }
            return OnExecuteConsumers(builder, info, interfaceName, generateFrom);
        }

        #endregion // OnExecute

        #region OnExecuteConsumers

        protected string OnExecuteConsumers(
            StringBuilder builder,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            string prefix = interfaceName.StartsWith("I") &&
                interfaceName.Length > 1 &&
                Char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;

            OnExecuteConsumerBase(builder, prefix, info, interfaceName, generateFrom);
            OnExecuteConsumerBridge(builder, prefix, info, interfaceName, generateFrom);
            OnExecuteConsumerBridgeExtensions(builder, prefix, info, interfaceName, generateFrom);
            return $"{prefix}.Subscription";
        }

        #endregion // OnExecuteConsumers

        #region OnExecuteConsumerBridgeExtensions

        protected void OnExecuteConsumerBridgeExtensions(
            StringBuilder builder,
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string bridge = $"{prefix}Bridge";
            string fileName = $"{bridge}Extensions";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge extensions for { interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
            builder.AppendLine($"\tpublic static class {fileName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets handler.</param>");
            builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
            builder.AppendLine("\t\t\t\tthis IConsumerSubscribeBuilder source,");
            builder.AppendLine($"\t\t\t\tparams {interfaceName}[] targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tvar bridge = new {bridge}(targets);");
            builder.AppendLine("\t\t\treturn source.Subscribe(bridge);");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets handler.</param>");
            builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
            builder.AppendLine("\t\t\t\tthis IConsumerSubscribeBuilder source,");
            builder.AppendLine($"\t\t\t\tIEnumerable<{interfaceName}> targets,");
            builder.AppendLine("\t\t\t\tstring? consumerGroup = null,");
            builder.AppendLine("\t\t\t\tstring? consumerName = null)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tvar bridge = new {bridge}(targets);");
            builder.AppendLine("\t\t\treturn source.Subscribe(bridge, consumerGroup, consumerName);");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"target\">A target handler.</param>");
            builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
            builder.AppendLine("\t\t\t\tthis IConsumerSubscribeBuilder source,");
            builder.AppendLine($"\t\t\t\t{interfaceName} target,");
            builder.AppendLine("\t\t\t\tstring? consumerGroup = null,");
            builder.AppendLine("\t\t\t\tstring? consumerName = null)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tvar bridge = new {bridge}(target);");
            builder.AppendLine("\t\t\treturn source.Subscribe(bridge, consumerGroup, consumerName);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t}");
        }

        #endregion // OnExecuteConsumerBridgeExtensions

        #region OnExecuteConsumerBridge

        protected void OnExecuteConsumerBridge(
            StringBuilder builder,
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string fileName = $"{prefix}Bridge";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge for { interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
            builder.AppendLine($"\tpublic sealed class {fileName}: ISubscriptionBridge");
            builder.AppendLine("\t{");

            builder.AppendLine($"\t\tprivate readonly IEnumerable<{interfaceName}> _targets;");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t///// </summary>");
            builder.AppendLine("\t\t/// <param name=\"target\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}({interfaceName} target)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = target.AsYield();");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t///// <summary>");
            builder.AppendLine("\t\t///// Initializes a new instance.");
            builder.AppendLine("\t\t///// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}(IEnumerable<{interfaceName}> targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = targets;");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t///// <summary>");
            builder.AppendLine("\t\t///// Initializes a new instance.");
            builder.AppendLine("\t\t///// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}(params {interfaceName}[] targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = targets;");
            builder.AppendLine("\t\t}");

            builder.AppendLine();

            builder.AppendLine("\t\tasync Task ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tswitch (announcement.Metadata.Operation)");
            builder.AppendLine("\t\t\t{");
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                string mtdName = mds.Identifier.ValueText;
                builder.AppendLine($"\t\t\t\tcase nameof({interfaceName}.{mtdName}):");
                builder.AppendLine("\t\t\t\t{");
                var prms = mds.ParameterList.Parameters;
                int i = 0;
                foreach (var p in prms)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\t\t\tvar p{i} = await consumerBridge.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }
                IEnumerable<string> ps = Enumerable.Range(0, prms.Count).Select(m => $"p{m}");

                builder.AppendLine($"\t\t\t\t\tvar tasks = _targets.Select(async target => await target.{mtdName}({string.Join(", ", ps)}));");
                builder.AppendLine("\t\t\t\t\tawait Task.WhenAll(tasks);");
                builder.AppendLine("\t\t\t\t\tbreak;");
                builder.AppendLine("\t\t\t\t}");
            }
            builder.AppendLine("\t\t\t}");
            builder.AppendLine("\t\t}");

            builder.AppendLine("\t}");
        }

        #endregion // OnExecuteConsumerBridge

        #region OnExecuteConsumerBase

        protected void OnExecuteConsumerBase(
            StringBuilder builder,
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string fileName = $"{prefix}Base";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Base Subscription class of {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
            builder.AppendLine($"\tpublic abstract class {fileName}: ISubscriptionBridge");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t async Task ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tswitch (announcement.Metadata.Operation)");
            builder.AppendLine("\t\t\t{");
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                string mtdName = mds.Identifier.ValueText;
                builder.AppendLine($"\t\t\t\tcase nameof({interfaceName}.{mtdName}):");
                builder.AppendLine("\t\t\t\t{");
                var prms = mds.ParameterList.Parameters;
                int i = 0;
                foreach (var p in prms)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\t\t\tvar p{i} = await consumerBridge.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }
                IEnumerable<string> ps = Enumerable.Range(0, prms.Count).Select(m => $"p{m}");
                builder.AppendLine($"\t\t\t\t\tawait {mtdName}({string.Join(", ", ps)});");
                builder.AppendLine("\t\t\t\t\tbreak;");
                builder.AppendLine("\t\t\t\t}");
            }
            builder.AppendLine("\t\t\t}");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                string mtdName = mds.Identifier.ValueText;

                CopyDocumentation(builder, kind, mds);
                var prms = mds.ParameterList.Parameters;
                IEnumerable<string> ps = prms.Select(p => $"{p.Type} {p.Identifier.ValueText}");
                builder.AppendLine($"\t\tprotected abstract ValueTask {mtdName}({string.Join(", ", ps)});"); ;
                builder.AppendLine();
            }
            builder.AppendLine("\t}");
        }

        #endregion // OnExecuteConsumerBase

        #region OnExecuteProducer

        protected string OnExecuteProducer(
            StringBuilder builder,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            var (item, att, name, kind, suffix, ns, isProducer) = info;
            builder.AppendLine("\tusing Weknow.EventSource.Backbone.Building;");

            // CopyDocumentation(builder, kind, item, "\t");
            string prefix = interfaceName.StartsWith("I") &&
                interfaceName.Length > 1 &&
                Char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
            string fileName = $"{prefix}BridgePipeline";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Bridge {kind} of {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"Weknow.EventSource.Backbone\",\"1.0\")]");
            builder.AppendLine($"\tinternal class {fileName}: ProducerPipeline, {interfaceName}");
            builder.AppendLine("\t{");

            builder.AppendLine($"\t\tpublic {fileName}(IProducerPlan plan) : base(plan){{}}");
            builder.AppendLine();
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;

                CopyDocumentation(builder, kind, mds);

                string mtdName = mds.Identifier.ValueText;
                builder.Append("\t\tasync ValueTask");
                if (isProducer)
                    builder.Append("<EventKeys>");
                builder.Append($" {interfaceName}.{mtdName}(");

                IEnumerable<string> ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                builder.Append("\t\t\t");
                builder.Append(string.Join(", ", ps));
                builder.AppendLine(")");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tvar operation = nameof({interfaceName}.{mtdName});");
                int i = 0;
                var prms = mds.ParameterList.Parameters;
                foreach (var p in prms)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\tvar classification{i} = CreateClassificationAdaptor(operation, nameof({pName}), {pName});");
                    i++;
                }
                var classifications = Enumerable.Range(0, prms.Count).Select(m => $"classification{m}");
                builder.AppendLine($"\t\t\treturn await SendAsync(operation, {string.Join(", ", classifications)});");
                builder.AppendLine("\t\t}");
                builder.AppendLine();

            }
            builder.AppendLine("\t}");

            // ========== Extensions ===============
            builder.AppendLine($"\tpublic static class {fileName}Extensions");
            builder.AppendLine("\t{");

            builder.AppendLine($"\t\tpublic static {interfaceName} Build{prefix}(");
            builder.AppendLine("\t\t\tthis IProducerSpecializeBuilder builder)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn builder.Build<{interfaceName}>(plan => new {fileName}(plan));");
            builder.AppendLine("\t\t}");

            builder.AppendLine($"\t\tpublic static {interfaceName} Build{prefix}(");
            builder.AppendLine($"\t\t\tthis IProducerOverrideBuildBuilder<{interfaceName}> builder)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn builder.Build(plan => new {fileName}(plan));");
            builder.AppendLine("\t\t}");

            builder.AppendLine("\t}");

            return fileName;
        }

        #endregion // OnExecuteProducer
    }
}