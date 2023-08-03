using System.Reflection;
using System.Text;

using EventSourcing.Backbone.SrcGen.Generators.Entities;
using EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

using Microsoft.CodeAnalysis;

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace EventSourcing.Backbone
{
    //[Generator]
    internal class BridgeIncrementalGenerator : GeneratorIncrementalBase
    {
        private const string TARGET_ATTRIBUTE = "EventsContractBridge";

        public BridgeIncrementalGenerator() : base(TARGET_ATTRIBUTE)
        {

        }

        #region OnGenerate

        protected override GenInstruction[] OnGenerate(
                            SourceProductionContext context,
                            Compilation compilation,
                            SyntaxReceiverResult info,
                            string[] usingStatements)
        {
            string interfaceName = info.FormatName();
            var builder = new StringBuilder();

            if (info.Kind == "Producer")
            {
                var file = OnGenerateProducer(compilation, builder, info, interfaceName, usingStatements);
                return new[] { new GenInstruction(file, builder.ToString()) };
            }
            return OnGenerateConsumers(compilation, info, interfaceName, usingStatements);

        }

        #endregion // OnGenerate

        #region OnGenerateConsumers

        protected GenInstruction[] OnGenerateConsumers(
                            Compilation compilation,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string[] usingStatements)
        {
            string generateFrom = info.FormatName();
            string namePrefix = (info.Name ?? interfaceName).ToClassName();

            AssemblyName assemblyName = GetType().Assembly.GetName();

            var dtos = EntityGenerator.GenerateEntities(compilation, namePrefix, info, interfaceName, assemblyName);
            GenInstruction[] gens =
            {
                EntityGenerator.GenerateEntityMapper(compilation, namePrefix, info, interfaceName , generateFrom, assemblyName),
                EntityGenerator.GenerateEntityMapperExtensions(compilation, namePrefix, info, interfaceName , generateFrom, assemblyName),
                OnGenerateConsumerBase(compilation, namePrefix, info, interfaceName, assemblyName),
                OnGenerateConsumerBridge(compilation, namePrefix, info, interfaceName, assemblyName),
                OnGenerateConsumerBridgeExtensions(compilation, namePrefix, info, interfaceName, generateFrom, assemblyName)
            };

            return dtos.Concat(gens).ToArray();
        }

        #endregion // OnGenerateConsumers

        #region OnGenerateConsumerBridgeExtensions

        protected GenInstruction OnGenerateConsumerBridgeExtensions(
                            Compilation compilation,
                            string prefix,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

            string bridge = $"{prefix}Bridge";
            string fileName = $"{bridge}Extensions";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge extensions for {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic static class {fileName}");
            builder.AppendLine("\t{");

            string[] variations = { "" }; //, $"<{interfaceName}>" };
            foreach (var variation in variations)
            {
                builder.AppendLine("\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
                builder.AppendLine("\t\t/// </summary>");
                builder.AppendLine("\t\t/// <param name=\"source\">The builder.</param>");
                builder.AppendLine("\t\t/// <param name=\"target\">The targets handler.</param>");
                builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
                builder.AppendLine($"\t\t\t\tthis IConsumerSubscriptionHubBuilder{variation} source,");
                builder.AppendLine($"\t\t\t\t{interfaceName} target)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tvar bridge = new {bridge}(target);");
                builder.AppendLine("\t\t\treturn source.Subscribe(bridge);");
                builder.AppendLine("\t\t}");
                builder.AppendLine();

                builder.AppendLine("\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Subscribe to {interfaceName}");
                builder.AppendLine("\t\t/// </summary>");
                builder.AppendLine("\t\t/// <param name=\"source\">The builder.</param>");
                builder.AppendLine("\t\t/// <param name=\"targets\">The targets handler.</param>");
                builder.AppendLine($"\t\tpublic static IConsumerLifetime Subscribe{prefix}(");
                builder.AppendLine($"\t\t\t\tthis IConsumerSubscriptionHubBuilder{variation} source,");
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
                builder.AppendLine($"\t\t\t\tthis IConsumerSubscriptionHubBuilder{variation} source,");
                builder.AppendLine($"\t\t\t\tIEnumerable<{interfaceName}> targets)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tvar bridge = new {bridge}(targets);");
                builder.AppendLine("\t\t\treturn source.Subscribe(bridge);");
                builder.AppendLine("\t\t}");
                builder.AppendLine();
            }

            builder.AppendLine("\t}");

            return new GenInstruction($"{prefix}.Consumer.Extensions", builder.ToString());
        }

        #endregion // OnGenerateConsumerBridgeExtensions

        #region OnGenerateConsumerBridge

        protected GenInstruction OnGenerateConsumerBridge(
                            Compilation compilation,
                            string prefix,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

            string fileName = $"{prefix}Bridge";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Subscription bridge for {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic sealed class {fileName}: {fileName}Base");
            builder.AppendLine("\t{");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"target\">The target is consumer implementation.</param>");
            builder.AppendLine($"\t\tpublic {fileName}({interfaceName} target): this (target.ToEnumerable())");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets are consumer implementations (when having multiple implementation on a single subscription).</param>");
            builder.AppendLine($"\t\tpublic {fileName}(IEnumerable<{interfaceName}> targets): this(targets.ToArray())");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The targets are consumer implementations (when having multiple implementation on a single subscription).</param>");
            builder.AppendLine($"\t\tpublic {fileName}(params {interfaceName}[] targets): base(targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            MethodBundle[] bundles = info.ToBundle(compilation);
            foreach (var bundle in bundles)
            {
                var method = bundle.Method;
                string nameVersion = bundle.FormatMethodFullName();

                var prms = method.Parameters;
                IEnumerable<string> ps = prms.Select(p => $"{p.Type} {p.Name}");
                IEnumerable<string> psName = prms.Select(p => p.Name);
                string metaParam = ps.Any() ? "ConsumerContext consumerContext, " : "ConsumerContext consumerContext";
                string metaParamName = ps.Any() ? "consumerContext, " : "consumerContext";
                builder.AppendLine($"\t\tpublic async override ValueTask {nameVersion}({metaParam}{string.Join(", ", ps)})");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tif(_targets.Length == 1)");
                builder.AppendLine("\t\t\t{");
                builder.AppendLine($"\t\t\t\tawait _targets[0].{nameVersion}({metaParamName}{string.Join(", ", psName)});");
                builder.AppendLine($"\t\t\t\treturn;");
                builder.AppendLine("\t\t\t}");
                builder.AppendLine("\t\t\tvar tasks = ");
                builder.AppendLine($"\t\t\t\t\t_targets.Select(async t => await t.{nameVersion}({metaParamName}{string.Join(", ", psName)}));");
                builder.AppendLine("\t\t\tawait Task.WhenAll(tasks).ThrowAll();");
                builder.AppendLine("\t\t}");
                builder.AppendLine();
            }
            builder.AppendLine("\t}");

            return new GenInstruction($"{prefix}.Subscription.Bridge", builder.ToString(), usingAddition: "EventSourcing.Backbone.Private");
        }

        #endregion // OnGenerateConsumerBridge

        #region OnGenerateConsumerBase

        protected GenInstruction OnGenerateConsumerBase(
                            Compilation compilation,
                            string prefix,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

            string fileName = $"{prefix}BridgeBase";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Base Subscription class of {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic abstract class {fileName}: ISubscriptionBridge<{interfaceName}>, {interfaceName}");
            builder.AppendLine("\t{");

            builder.AppendLine();
            builder.AppendLine($"\t\tprotected readonly {interfaceName}[] _targets;");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine("\t\t/// Initializes a new instance.");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine("\t\t/// <param name=\"targets\">The target.</param>");
            builder.AppendLine($"\t\tpublic {fileName}(params {interfaceName}[] targets)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_targets = targets;");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            builder.AppendLine($"\t\t{interfaceName} ISubscriptionBridge<{interfaceName}>.Consumer => this;");
            builder.AppendLine();

            builder.Append("\t\t");

            var fallbackNames = info.Symbol.GetInterceptors(interfaceName);

            MethodBundle[] bundles = info.ToBundle(compilation);
            if (bundles.Length != 0)
                builder.Append("async ");
            builder.AppendLine("Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)");
            builder.AppendLine("\t\t{");
            if (bundles.Length != 0)
            {
                builder.AppendLine("\t\t\tConsumerContext consumerContext = ConsumerContext.Context;");
                builder.AppendLine("\t\t\tMetadata meta = announcement.Metadata;");
                builder.AppendLine("\t\t\tswitch (meta)");
                builder.AppendLine("\t\t\t{");
                foreach (var bundle in bundles)
                {
                    var method = bundle.Method;
                    var v = bundle.Version;
                    var paramsSignature = method.GetParamsSignature();

                    string mtdName = bundle.FormatMethodFullName();

                    string nameOfOperetion = bundle.FullName;
                    builder.AppendLine($"\t\t\t\tcase {{ Operation: \"{nameOfOperetion}\", Version: {v}, ParamsSignature: \"{paramsSignature}\" }} :");
                    builder.AppendLine("\t\t\t\t{");
                    var prms = method.Parameters;
                    int i = 0;
                    foreach (var p in prms)
                    {
                        var pName = p.Name;
                        builder.AppendLine($"\t\t\t\t\tvar p{i} = await consumerBridge.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                        i++;
                    }
                    string metaParam = prms.Any() ? "consumerContext, " : "consumerContext";
                    IEnumerable<string> ps = Enumerable.Range(0, prms.Length).Select(m => $"p{m}");
                    builder.AppendLine($"\t\t\t\t\tawait {mtdName}({metaParam}{string.Join(", ", ps)});");
                    builder.AppendLine("\t\t\t\t\treturn true;");
                    builder.AppendLine("\t\t\t\t}");
                }
                builder.AppendLine("\t\t\t}");
                builder.AppendLine();
                builder.AppendLine("\t\t\tvar fallbackHandle = new ConsumerInterceptionContext(announcement, consumerBridge, consumerContext);");
                builder.AppendLine("\t\t\tbool result = false;");
                foreach (string fallbackName in fallbackNames)
                { 
                    builder.AppendLine($"\t\t\tresult = await {interfaceName}.{fallbackName}(fallbackHandle, this);");
                }
                builder.AppendLine();
                builder.AppendLine("\t\t\treturn result;");
            }
            else
                builder.AppendLine("\t\t\treturn Task.FromResult(false);");
            builder.AppendLine("\t\t}");

            builder.AppendLine();
            foreach (var bundle in bundles)
            {
                var method = bundle.Method;
                string mtdName = bundle.FormatMethodFullName();

                var prms = method.Parameters;
                IEnumerable<string> ps = prms.Select(p => $"{p.Type} {p.Name}");
                string metaParam = ps.Any() ? "ConsumerContext consumerContext, " : "ConsumerContext consumerContext";
                builder.AppendLine($"\t\tpublic abstract ValueTask {mtdName}({metaParam}{string.Join(", ", ps)});");
                builder.AppendLine();
            }
            builder.AppendLine("\t}");

            return new GenInstruction($"{prefix}.Subscription.Bridge.Base", builder.ToString());
        }

        #endregion // OnGenerateConsumerBase

        #region OnGenerateProducer

        protected string OnGenerateProducer(
                            Compilation compilation,
                            StringBuilder builder,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string[] usingStatements)
        {
            var kind = info.Kind;

            string prefix = interfaceName.StartsWith("I") &&
                interfaceName.Length > 1 &&
                char.IsUpper(interfaceName[1]) ? interfaceName.Substring(1) : interfaceName;
            string fileName = $"{prefix}BridgePipeline";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Bridge {kind} of {interfaceName}");
            builder.AppendLine("\t/// </summary>");
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

            var bundles = info.ToBundle(compilation);
            foreach (var bundle in bundles)
            {
                GenerateProducerMethods(bundle, info, builder, interfaceName);
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

        #region GenerateProducerMethods

        private static void GenerateProducerMethods(
                            MethodBundle bundle,
                            SyntaxReceiverResult info,
                            StringBuilder builder,
                            string interfaceName)
        {
            var method = bundle.Method;
            string mtdName = method.ToNameConvention();
            string nameVersion = bundle.FormatMethodFullName(mtdName);
            string interfaceNameFormatted = info.FormatName(interfaceName);
            builder.Append("\t\tasync ValueTask");
            builder.Append("<EventKeys>");
            builder.Append($" {interfaceNameFormatted}.{nameVersion}(");

            var paramsSignature = method.GetParamsSignature();

            IEnumerable<string> ps = method.Parameters.Select(p => $"\r\n\t\t\t{p.Type} {p.Name}");
            builder.Append("\t\t\t");
            builder.Append(string.Join(", ", ps));
            builder.AppendLine(")");
            builder.AppendLine("\t\t{");

            string nameOfOperetion = mtdName;
            builder.AppendLine($"\t\t\tvar operation_ = \"{nameOfOperetion}\";");
            builder.AppendLine($"\t\t\tvar version_ = {bundle.Version};");
            builder.AppendLine($"\t\t\tvar prms_ = \"{paramsSignature}\";");
            int i = 0;
            var prms = method.Parameters;
            foreach (var pName in from p in prms
                                  let pName = p.Name
                                  select pName)
            {
                builder.AppendLine($"\t\t\tvar classification_{i}_ = CreateClassificationAdapter(operation_, nameof({pName}), {pName});");
                i++;
            }

            var classifications = Enumerable.Range(0, prms.Length).Select(m => $"classification_{m}_");

            builder.Append($"\t\t\treturn await SendAsync(operation_, version_, prms_");
            if (classifications.Any())
                builder.AppendLine($", {string.Join(", ", classifications)});");
            else
                builder.AppendLine($");");
            builder.AppendLine("\t\t}");
            builder.AppendLine();
        }

        #endregion // GenerateProducerMethods
    }
}