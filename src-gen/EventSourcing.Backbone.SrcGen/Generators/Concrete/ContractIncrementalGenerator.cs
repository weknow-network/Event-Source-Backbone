using System.Text;

using EventSourcing.Backbone.SrcGen.Entities;
using EventSourcing.Backbone.SrcGen.Generators.Entities;
using EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EventSourcing.Backbone;

[Generator]
internal class ContractIncrementalGenerator : GeneratorIncrementalBase
{
    private const string TARGET_ATTRIBUTE = "EventsContract";
    private readonly BridgeIncrementalGenerator _bridge = new();

    public ContractIncrementalGenerator() : base(TARGET_ATTRIBUTE)
    {

    }

    /// <summary>
    /// Called when [execute].
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="compilation"></param>
    /// <param name="info">The information.</param>
    /// <returns>
    /// File name
    /// </returns>
    protected override GenInstruction[] OnGenerate(
                        SourceProductionContext context,
                        Compilation compilation,
                        SyntaxReceiverResult info,
                        string[] usingStatements)
    {
#pragma warning disable S1481 // Unused local variables should be removed
        var (type, att, symbol, kind, ns, isProducer, @using) = info;
#pragma warning restore S1481 // Unused local variables should be removed
        string interfaceName = info.FormatName();
        string clsPrefix = interfaceName.ToClassName();

        VersionInstructions versionInfo = att.GetVersionInfo(compilation, info.Kind);

        var bundles = info.ToBundle(compilation);
        int? maxVersion = bundles.Max(m => m.Version);
        var builder = new StringBuilder();
        symbol.CopyDocumentation(builder, maxVersion, "\t");
        var asm = GetType().Assembly.GetName();
        builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
        builder.AppendLine($"\tpublic interface {interfaceName}");
        builder.AppendLine("\t{");

        foreach (var bundle in bundles)
        {
            IMethodSymbol method = bundle.Method;
            var opVersionInfo = method.GetOperationVersionInfo();
            var v = opVersionInfo.Version;
            if (versionInfo.MinVersion > v || versionInfo.IgnoreVersion.Contains(v))
                continue;

            versionInfo = GenMethod(method, isProducer, versionInfo, builder, opVersionInfo);
        }
        if (kind == "Consumer")
        {
            builder.AppendLine($"\t\tpublic static {clsPrefix}_Constants Constants {{ get; }} = new {clsPrefix}_Constants();");
        }

        builder.AppendLine("\t}");


        var contractOnlyArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "ContractOnly");
        var contractOnly = contractOnlyArg?.Expression.NormalizeWhitespace().ToString() == "true";

        if (!contractOnly)
            _bridge.GenerateSingle(context, compilation, info);
        var constants = GenVersionConstants();
        if (constants != null)
            return new[] { constants, new GenInstruction(interfaceName, builder.ToString()) };
        return new[] { new GenInstruction(interfaceName, builder.ToString()) };

        #region GenVersionConstants

        GenInstruction? GenVersionConstants()
        {
            if (kind != "Consumer")
                return null;
            var builder = new StringBuilder();
            GenVersionConstants(builder);
            return new GenInstruction($"{interfaceName}.Constants", builder.ToString());

            void GenVersionConstants(StringBuilder builder)
            {
                MethodBundle[] bundles = info.ToBundle(compilation, true);

                string indent = "\t";
                builder.AppendLine($"{indent}public class {clsPrefix}_Constants");
                builder.AppendLine($"{indent}{{");

                var active = bundles.Where(b => !b.Deprecated);
                GenConstantsOperations(active, indent);

                indent = $"{indent}\t";
                builder.AppendLine($"{indent}public Deprecated_Constants Deprecated {{ get; }} = new Deprecated_Constants();");
                builder.AppendLine($"{indent}public class Deprecated_Constants");
                builder.AppendLine($"{indent}{{");
                var deprecated = bundles.Where(b => b.Deprecated);
                GenConstantsOperations(deprecated, indent);
                builder.AppendLine($"{indent}}}");

                void GenConstantsOperations(IEnumerable<MethodBundle> bundles, string indent)
                {

                    indent = $"{indent}\t";
                    var groupd = bundles.GroupBy(b => b.FullName);
                    foreach (var group in groupd)
                    {
                        builder.AppendLine($"{indent}public {group.Key}_Constants {group.Key} {{ get; }} = new {group.Key}_Constants();");

                        builder.AppendLine($"{indent}public class {group.Key}_Constants");
                        builder.AppendLine($"{indent}{{");

                        indent = $"{indent}\t";
                        var versions = group.GroupBy(b => b.Version);
                        foreach (var version in versions)
                        {
                            builder.AppendLine($"{indent}public V{version.Key}_Constants V{version.Key} {{ get; }} = new V{version.Key}_Constants();");

                            builder.AppendLine($"{indent}public class V{version.Key}_Constants");
                            builder.AppendLine($"{indent}{{");

                            indent = $"{indent}\t";
                            foreach (var b in version)
                            {
                                string pName = b.Parameters.Replace(",", "_");
                                builder.AppendLine($"{indent}public string P_{pName} {{ get; }} = \"{b.Parameters}\";");
                            }

                            indent = indent.Substring(1);
                            builder.AppendLine($"{indent}}}");
                        }
                        indent = indent.Substring(1);

                        builder.AppendLine($"{indent}}}");
                    }
                }
                indent = indent.Substring(1);
                builder.AppendLine($"{indent}}}");
            }
        }

        #endregion // GenVersionConstants

        #region GetParameter

        string GetParameter(IParameterSymbol p)
        {
            var isPrmArray = p.IsParams;
            string @params = isPrmArray ? "params " : string.Empty;

            var result = $"\r\n\t\t\t{@params}{p.Type} {p.Name}";
            return result;
        }

        #endregion // GetParameter

        #region GenMethod

        VersionInstructions GenMethod(
            IMethodSymbol method,
            bool isProducer,
            VersionInstructions versionInfo,
            StringBuilder builder,
            OperatioVersionInstructions opVersionInfo)
        {
            var sb = new StringBuilder();
            int version = opVersionInfo.Version;
            method.CopyDocumentation(sb, version, "\t\t");
            var ps = method.Parameters.Select(GetParameter);
            if (sb.Length != 0 && !isProducer && ps.Any())
            {
                string summaryEnds = "</summary>";
                int idxRet = sb.ToString().IndexOf(summaryEnds);
                if (idxRet != -1)
                    sb.Insert(idxRet + summaryEnds.Length, "\r\n\t\t/// <param name=\"consumerMetadata\">The consumer metadata.</param>");
            }
            builder.Append(sb);

            builder.Append("\t\tValueTask");
            if (isProducer)
                builder.Append("<EventKeys>");
            var mtdName = method.ToNameConvention();
            string nameVersion = versionInfo.FormatMethodName(mtdName, version);
            builder.AppendLine($" {nameVersion}(");

            if (!isProducer)
            {
                builder.Append("\t\t\t");
                builder.Append("ConsumerMetadata consumerMetadata");
                if (ps.Any())
                    builder.Append(',');
            }
            builder.Append("\t\t\t");
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            builder.Append(string.Join(",", ps));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            builder.AppendLine(");");
            builder.AppendLine();
            return versionInfo;
        }

        #endregion // GenMethod
    }
}
