using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Weknow.EventSource.Backbone.SrcGen.Generators.Entities;
using Weknow.EventSource.Backbone.SrcGen.Generators.EntitiesAndHelpers;
using Weknow.EventSource.Backbone.SrcGen.Generators.Sync;
using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone
{
    [Generator]
    internal class BridgeIncrementalGenerator : GeneratorIncrementalBase
    {
        private const string TARGET_ATTRIBUTE = "GenerateEventSourceBridge";

        public BridgeIncrementalGenerator() : base(TARGET_ATTRIBUTE)
        {

        }

        #region OnGenerate

        protected override GenInstruction[] OnGenerate(
                            SourceProductionContext context,
                            Compilation compilation,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            var verrideInterfaceArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "InterfaceName");
            var overrideInterfaceName = verrideInterfaceArg?.Expression.NormalizeWhitespace().ToString().Replace("\"", "");

            if (info.Kind == "Producer")
            {
                var file = OnGenerateProducer(builder, info, interfaceName, generateFrom);
                return new[] { new GenInstruction(file, builder.ToString()) };
            }
            return OnGenerateConsumers(info, interfaceName, generateFrom);

        }

        #endregion // OnGenerate

        #region OnGenerateConsumers

        protected GenInstruction[] OnGenerateConsumers(
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom)
        {
            string prefix = (info.Name ?? interfaceName).StartsWith("I") &&
                interfaceName.Length > 1 &&
                char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;

            AssemblyName assemblyName = GetType().Assembly.GetName();

            var dtos = EntityGenerator.GenerateEntities(prefix, info, interfaceName, generateFrom, assemblyName);
            GenInstruction[] gens =
            {
                EntityGenerator.GenerateEntityFamilyContract(prefix, info, interfaceName , generateFrom, assemblyName),
                EntityGenerator.GenerateEntityMapper(prefix, info, interfaceName , generateFrom, assemblyName),
                EntityGenerator.GenerateEntityMapperExtensions(prefix, info, interfaceName , generateFrom, assemblyName),
                OnGenerateConsumerBase(prefix, info, interfaceName, generateFrom, assemblyName),
                OnGenerateConsumerBridge(prefix, info, interfaceName, generateFrom, assemblyName),
                OnGenerateConsumerBridgeExtensions(prefix, info, interfaceName, generateFrom, assemblyName)
            };

            return dtos.Concat(gens).ToArray();
        }

        #endregion // OnGenerateConsumers

        #region OnGenerateConsumerBridgeExtensions

        protected GenInstruction OnGenerateConsumerBridgeExtensions(
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string bridge = $"{prefix}Bridge";
            string fileName = $"{bridge}Extensions";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge extensions for {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic static class {fileName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"source\">The builder.</param>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets handler.</param>");
            builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
            builder.AppendLine("\t\t\t\tthis IConsumerSubscribtionHubBuilder source,");
            builder.AppendLine($"\t\t\t\tparams {interfaceName}[] targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tvar bridge = new {bridge}(targets);");
            builder.AppendLine("\t\t\treturn source.Subscribe(bridge);");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"source\">The builder.</param>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets handler.</param>");
            builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
            builder.AppendLine("\t\t\t\tthis IConsumerSubscribtionHubBuilder source,");
            builder.AppendLine($"\t\t\t\tIEnumerable<{interfaceName}> targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tvar bridge = new {bridge}(targets);");
            builder.AppendLine("\t\t\treturn source.Subscribe(bridge);");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            builder.AppendLine("\t}");

            return new GenInstruction($"{prefix}.Consumer.Extensions", builder.ToString());
        }

        #endregion // OnGenerateConsumerBridgeExtensions

        #region OnGenerateConsumerBridge

        protected GenInstruction OnGenerateConsumerBridge(
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string fileName = $"{prefix}Bridge";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge for {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic sealed class {fileName}: ISubscriptionBridge");
            builder.AppendLine("\t{");

            builder.AppendLine($"\t\tprivate readonly IEnumerable<{interfaceName}> _targets;");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"target\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}({interfaceName} target)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = target.ToYield();");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}(IEnumerable<{interfaceName}> targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = targets;");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}(params {interfaceName}[] targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = targets;");
            builder.AppendLine("\t\t}");

            builder.AppendLine();

            builder.Append("\t\t");
            if (item.Members.Count != 0)
                builder.Append("async ");
            builder.AppendLine("Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)");
            builder.AppendLine("\t\t{");
            if (item.Members.Count != 0)
            {
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
                    builder.AppendLine("\t\t\t\t\treturn true;");
                    builder.AppendLine("\t\t\t\t}");
                }
                builder.AppendLine("\t\t\t}");
            }
            if (item.Members.Count == 0)
                builder.AppendLine("\t\t\treturn Task.FromResult(false);");
            else
                builder.AppendLine("\t\t\treturn false;");
            builder.AppendLine("\t\t}");

            builder.AppendLine("\t}");
            return new GenInstruction($"{prefix}.Subscription.Bridge", builder.ToString());
        }

        #endregion // OnGenerateConsumerBridge

        #region OnGenerateConsumerBase

        protected GenInstruction OnGenerateConsumerBase(
            string prefix,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string fileName = $"{prefix}Base";

            builder.AppendLine("\tnamespace Hidden");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Base Subscription class of {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\t/// <inheritdoc cref=\"{generateFrom}\" />");
            builder.AppendLine($"\t\t[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\t\tpublic abstract class {fileName}: ISubscriptionBridge");
            builder.AppendLine("\t\t{");

            builder.Append("\t\t\t");
            if (item.Members.Count != 0)
                builder.Append("async ");
            builder.AppendLine("Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)");
            builder.AppendLine("\t\t\t{");
            if (item.Members.Count != 0)
            {
                builder.AppendLine("\t\t\t\tswitch (announcement.Metadata.Operation)");
                builder.AppendLine("\t\t\t\t{");
                foreach (var method in item.Members)
                {
                    if (method is not MethodDeclarationSyntax mds)
                        continue;
                    string mtdName = mds.Identifier.ValueText;
                    builder.AppendLine($"\t\t\t\t\tcase nameof({interfaceName}.{mtdName}):");
                    builder.AppendLine("\t\t\t\t\t{");
                    var prms = mds.ParameterList.Parameters;
                    int i = 0;
                    foreach (var p in prms)
                    {
                        var pName = p.Identifier.ValueText;
                        builder.AppendLine($"\t\t\t\t\t\tvar p{i} = await consumerBridge.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                        i++;
                    }
                    IEnumerable<string> ps = Enumerable.Range(0, prms.Count).Select(m => $"p{m}");
                    builder.AppendLine($"\t\t\t\t\t\tawait {mtdName}({string.Join(", ", ps)});");
                    builder.AppendLine("\t\t\t\t\t\treturn true;");
                    builder.AppendLine("\t\t\t\t\t}");
                }
                builder.AppendLine("\t\t\t\t}");
                builder.AppendLine("\t\t\t\treturn false;");
            }
            else
                builder.AppendLine("\t\t\t\treturn Task.FromResult(false);");
            builder.AppendLine("\t\t\t}");

            builder.AppendLine();
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                string mtdName = mds.Identifier.ValueText;

                CopyDocumentation(builder, kind, mds);
                var prms = mds.ParameterList.Parameters;
                IEnumerable<string> ps = prms.Select(p => $"{p.Type} {p.Identifier.ValueText}");
                builder.AppendLine($"\t\t\tprotected abstract ValueTask {mtdName}({string.Join(", ", ps)});"); ;
                builder.AppendLine();
            }
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t}");

            return new GenInstruction($"{prefix}.Subscription.Bridge.Base", builder.ToString());
        }

        #endregion // OnGenerateConsumerBase

        #region OnGenerateProducer

        protected string OnGenerateProducer(
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
                char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
            string fileName = $"{prefix}BridgePipeline";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Bridge {kind} of {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t/// <inheritdoc cref=\"{generateFrom}\" />");
            var asm = GetType().Assembly.GetName();
            builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
            builder.AppendLine($"\tpublic class {fileName}: ProducerPipeline, {interfaceName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Bridge Factory {kind} of {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\tpublic static {interfaceName} Create(IProducerPlan plan) => new {fileName}(plan);");
            builder.AppendLine();

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Bridge Constructor");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\tprivate {fileName}(IProducerPlan plan) : base(plan){{}}");
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
            builder.AppendLine($"\t\t\treturn builder.Build<{interfaceName}>(plan => {fileName}.Create(plan));");
            builder.AppendLine("\t\t}");

            builder.AppendLine($"\t\tpublic static {interfaceName} Build{prefix}(");
            builder.AppendLine($"\t\t\tthis IProducerOverrideBuildBuilder<{interfaceName}> builder)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn builder.Build(plan => {fileName}.Create(plan));");
            builder.AppendLine("\t\t}");

            builder.AppendLine("\t}");

            return fileName;
        }

        #endregion // OnGenerateProducer
    }
}