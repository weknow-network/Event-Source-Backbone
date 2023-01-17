using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Weknow.EventSource.Backbone.SrcGen.Generators.Entities;

using static Weknow.EventSource.Backbone.Helper;

namespace Weknow.EventSource.Backbone.SrcGen.Generators.EntitiesAndHelpers
{
    internal static class EntityGenerator
    {
        #region GenerateEntities    

        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="friendlyName">Name of the interface.</param>
        /// <param name="context">The context.</param>
        /// <param name="info">The information.</param>
        /// <param name="generateFrom"></param>
        /// <returns>
        /// File name
        /// </returns>
        internal static GenInstruction[] GenerateEntities(
            string friendlyName,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {

            var (item, att, name, kind, suffix, ns, isProducer) = info;

            var results = new List<GenInstruction>();
            foreach (var method in item.Members)
            {
                if (method is MethodDeclarationSyntax mds)
                {
                    var builder = new StringBuilder();
                    CopyDocumentation(builder, kind, mds, "\t");

                    string recordPrefix = friendlyName;
                    if (recordPrefix.EndsWith(nameof(KindFilter.Consumer)))
                        recordPrefix = recordPrefix.Substring(0, recordPrefix.Length - nameof(KindFilter.Consumer).Length);

                    string mtdName = mds.Identifier.ValueText;
                    if (mtdName.EndsWith("Async"))
                        mtdName = mtdName.Substring(0, mtdName.Length - 5);
                    builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
                    builder.Append("\tpublic record");
                    builder.Append($" {recordPrefix}_{mtdName}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"{Environment.NewLine}\t\t\t{p.Type} {p.Identifier.ValueText}");
                    builder.Append("\t\t");
                    builder.Append(string.Join(", ", ps));
                    builder.AppendLine($"): {interfaceName}_EntityFamily;");
                    builder.AppendLine();

                    results.Add(new GenInstruction($"{recordPrefix}.{mtdName}.Entity", builder.ToString()));
                }
            }


            return results.ToArray();
        }

        #endregion // GenerateEntities   

        #region GenerateEntityFamilyContract

        internal static GenInstruction GenerateEntityFamilyContract(
            string friendlyName,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Marker interface for entity mapper family contract generated from {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\t[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic interface {interfaceName}_EntityFamily");
            builder.AppendLine("\t{");
            builder.AppendLine("\t}");

            return new GenInstruction($"{interfaceName}.FamilyContract", builder.ToString());
        }

        #endregion // GenerateEntityFamilyContract

        #region GenerateEntityMapper

        internal static GenInstruction GenerateEntityMapper(
            string friendlyName,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            //builder.AppendLine("\tnamespace Hidden");
            //builder.AppendLine("\t{");


            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Entity mapper is responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\t[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\t\tpublic sealed class {friendlyName}EntityMapper: IConsumerEntityMapper<{interfaceName}_EntityFamily>");
            builder.AppendLine("\t\t{");

            builder.AppendLine("\t\t\t/// <summary>");
            builder.AppendLine($"\t\t\t/// Singleton entity mapper which responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t\t/// </summary>");
            builder.AppendLine($"\t\t\tinternal static readonly IConsumerEntityMapper<{interfaceName}_EntityFamily> Default = new {friendlyName}EntityMapper();");


            builder.AppendLine();
            builder.AppendLine($"\t\t\tprivate {friendlyName}EntityMapper() {{}}");
            builder.AppendLine();

            string recordPrefix = friendlyName;
            if (recordPrefix.EndsWith(nameof(KindFilter.Consumer)))
                recordPrefix = recordPrefix.Substring(0, recordPrefix.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine("\t\t\t/// <summary>");
            builder.AppendLine($"\t\t\t///  Try to map announcement");
            builder.AppendLine("\t\t\t/// </summary>");
            builder.AppendLine("\t\t\t/// <typeparam name=\"TCast\">Cast target</typeparam>");
            builder.AppendLine("\t\t\t/// <param name=\"announcement\">The announcement.</param>");
            builder.AppendLine("\t\t\t/// <param name=\"consumerPlan\">The consumer plan.</param>");
            if (item.Members.Count == 0)
                builder.Append($"\t\t\t public ");
            else
                builder.Append($"\t\t\t public async ");

            builder.AppendLine($"Task<(TCast? value, bool succeed)> TryMapAsync<TCast>(");
            builder.AppendLine($"\t\t\t\t\tAnnouncement announcement, ");
            builder.AppendLine($"\t\t\t\t\tIConsumerPlan consumerPlan)");
            builder.AppendLine($"\t\t\t\t\t\t where TCast : {interfaceName}_EntityFamily");
            builder.AppendLine("\t\t\t{");
            builder.AppendLine("\t\t\t\tvar operation = announcement.Metadata.Operation;");

            int j = 0;
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                string mtdName = mds.Identifier.ValueText;
                string recordSuffix = mtdName.EndsWith("Async") ? mtdName.Substring(0, mtdName.Length - 5) : mtdName;
                string fullRecordName = $"{recordPrefix}_{recordSuffix}";

                string nameOfOperetion = $"{info.Name ?? interfaceName}.{mtdName}";
                string ifOrElseIf = j++ > 0 ? "else if" : "if";
                builder.AppendLine($"\t\t\t\t{ifOrElseIf}(operation == nameof({nameOfOperetion}))");
                builder.AppendLine("\t\t\t\t{");
                builder.AppendLine($"\t\t\t\t\tif(typeof(TCast) == typeof({fullRecordName}))");
                builder.AppendLine("\t\t\t\t\t{");
                var prms = mds.ParameterList.Parameters;
                int i = 0;
                foreach (var p in prms)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\t\t\t\tvar p{i} = await consumerPlan.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }
                var ps = Enumerable.Range(0, prms.Count).Select(m => $"p{m}");

                builder.AppendLine($"\t\t\t\t\t\t{interfaceName}_EntityFamily rec = new {fullRecordName}({string.Join(", ", ps)});");
                builder.AppendLine($"\t\t\t\t\t\treturn ((TCast?)rec, true);");
                builder.AppendLine("\t\t\t\t\t}");
                builder.AppendLine("\t\t\t\t}");
            }
            if (item.Members.Count == 0)
                builder.AppendLine($"\t\t\t\treturn Task.FromResult<(TCast? value, bool succeed)>((default, false));");
            else
                builder.AppendLine($"\t\t\t\treturn (default, false);");
            builder.AppendLine("\t\t\t}");

            builder.AppendLine("\t\t}");
            //builder.AppendLine("\t}");

            return new GenInstruction($"{friendlyName}.EntityMapper", builder.ToString());
        }

        #endregion // GenerateEntityMapper

        #region GenerateEntityMapperExtensions

        internal static GenInstruction GenerateEntityMapperExtensions(
            string friendlyName,
            SyntaxReceiverResult info,
            string interfaceName,
            string generateFrom,
            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, name, kind, suffix, ns, isProducer) = info;

            // CopyDocumentation(builder, kind, item, "\t");

            string bridge = $"{friendlyName}EntityMapper";
            string fileName = $"{bridge}Extensions";

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Entity mapper is responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic static class {fileName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Specialize Enumerator of event produced by {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");

            builder.AppendLine($"\t\tpublic static IConsumerIterator<{interfaceName}_EntityFamily> Specialize{friendlyName} (this IConsumerIterator iterator)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn iterator.Specialize({bridge}.Default);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t}");

            return new GenInstruction($"{friendlyName}.EntityMapper.Extensions", builder.ToString());
        }

        #endregion // GenerateEntityMapperExtensions
    }
}
