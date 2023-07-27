using System.Text;

using EventSourcing.Backbone.SrcGen.Entities;
using EventSourcing.Backbone.SrcGen.Generators.Entities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static EventSourcing.Backbone.Helper;

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
        VersionInstructions versionInfo = att.GetVersionInfo(compilation, info.Kind);

        var builder = new StringBuilder();
        CopyDocumentation(builder, kind, type, "\t");
        var asm = GetType().Assembly.GetName();
        builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
        builder.Append($"\tpublic interface {interfaceName}");
        var baseTypesFormats = symbol.Interfaces.Select(m => info.FormatName(m.Name));
        string inheritanceNames = string.Join(", ", baseTypesFormats);
        if (string.IsNullOrEmpty(inheritanceNames))
            builder.AppendLine();
        else
            builder.AppendLine($" : {inheritanceNames}");
        builder.AppendLine("\t{");
        foreach (MemberDeclarationSyntax method in type.Members)
        {
            if (method is MethodDeclarationSyntax mds)
            {
                var opVersionInfo = mds.GetOperationVersionInfo(compilation);
                var v = opVersionInfo.Version;
                if (versionInfo.MinVersion > v || versionInfo.IgnoreVersion.Contains(v))
                    continue;

                versionInfo = GenMethod(kind, isProducer, versionInfo, builder, mds, opVersionInfo);
            }
        }
        builder.AppendLine("\t}");

        var contractOnlyArg = att.ArgumentList?.Arguments.FirstOrDefault(m => m.NameEquals?.Name.Identifier.ValueText == "ContractOnly");
        var contractOnly = contractOnlyArg?.Expression.NormalizeWhitespace().ToString() == "true";

        if (!contractOnly)
            _bridge.GenerateSingle(context, compilation, info);

        return new[] { new GenInstruction(interfaceName, builder.ToString()) };

        #region GetParameter

        string GetParameter(ParameterSyntax p)
        {
            var mod = p.Modifiers.FirstOrDefault();
            string modifier = mod == null ? string.Empty : $" {mod} ";
            var result = $"\r\n\t\t\t{modifier}{p.Type} {p.Identifier.ValueText}";
            return result;
        }

        #endregion // GetParameter

        #region GenMethod

        VersionInstructions GenMethod(
            string kind,
            bool isProducer,
            VersionInstructions versionInfo,
            StringBuilder builder,
            MethodDeclarationSyntax mds,
            OperatioVersionInstructions opVersionInfo)
        {
            var sb = new StringBuilder();
            int version = opVersionInfo.Version;
            CopyDocumentation(compilation, sb, kind, mds, version);
            var ps = mds.ParameterList.Parameters.Select(GetParameter);
            if (sb.Length != 0 && !isProducer && ps.Any())
            {
                string summaryEnds = "/// </summary>";
                int idxRet = sb.ToString().IndexOf(summaryEnds);
                if (idxRet != -1)
                    sb.Insert(idxRet + summaryEnds.Length, "\r\n\t\t/// <param name=\"consumerMetadata\">The consumer metadata.</param>");
            }
            builder.Append(sb);

            builder.Append("\t\tValueTask");
            if (isProducer)
                builder.Append("<EventKeys>");
            var mtdName = mds.ToNameConvention();
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
