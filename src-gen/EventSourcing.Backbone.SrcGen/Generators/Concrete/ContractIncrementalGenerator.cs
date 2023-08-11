using System.Text;
using EventSourcing.Backbone.SrcGen.Entities;

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

        var bundles = info.ToBundle(compilation);
        int? maxVersion = bundles.Length == 0 ? 0 : bundles.Max(m => m.Version);
        var builder = new StringBuilder();
        symbol.CopyDocumentation(builder, maxVersion, "\t");
        var asm = GetType().Assembly.GetName();
        builder.AppendLine($"\t[GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
        builder.AppendLine($"\tpublic interface {interfaceName}");
        builder.AppendLine("\t{");

        // Copy Fallback handler
        // builder.AddInterceptors(symbol, interfaceName);

        foreach (MethodBundle bundle in bundles)
        {
            if (bundle.Deprecated)
                continue;
            
            GenMethod(builder, bundle);
        }

        builder.AppendLine("\t}");

        _bridge.GenerateSingle(context, compilation, info);
        return new[] { new GenInstruction(interfaceName, builder.ToString()) };

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

        void GenMethod(
            StringBuilder builder,
            MethodBundle bundle)
        {
            var sb = new StringBuilder();
            var method = bundle.Method;
            int version = bundle.Version;
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
            string nameVersion = bundle.FormatMethodFullName(mtdName);
            builder.AppendLine($" {nameVersion}(");

            if (!isProducer)
            {
                builder.Append("\t\t\t");
                builder.Append("ConsumerContext consumerMetadata");
                if (ps.Any())
                    builder.Append(',');
            }
            builder.Append("\t\t\t");
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            builder.Append(string.Join(",", ps));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            builder.AppendLine(");");
            builder.AppendLine();
        }

        #endregion // GenMethod
    }
}
