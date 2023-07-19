using System.Reflection;
using System.Text;

using EventSourcing.Backbone.SrcGen.Generators.Entities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static EventSourcing.Backbone.Helper;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers
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
                            Compilation compilation,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
        {

            var (item, att, symbol, kind, ns, usingStatements) = info;
            var versionInfo = att.GetVersionInfo(compilation, info.Kind);


            // TODO: [bnaya 2023-07-16] find max (current) version and excludes retired
            var results = new List<GenInstruction>();
            foreach (var method in item.Members)
            {
                if (method is MethodDeclarationSyntax mds)
                {
                    var opVersionInfo = mds.GetOperationVersionInfo(compilation);
                    opVersionInfo.Parent = versionInfo;
                    var v = opVersionInfo.Version;
                    if (versionInfo.MinVersion > v || versionInfo.IgnoreVersion.Contains(v))
                        continue;

                    string version = opVersionInfo.ToString();

                    var builder = new StringBuilder();
                    CopyDocumentation(builder, kind, mds, opVersionInfo, "\t");

                    string recordPrefix = friendlyName;
                    if (recordPrefix.EndsWith(nameof(KindFilter.Consumer)))
                        recordPrefix = recordPrefix.Substring(0, recordPrefix.Length - nameof(KindFilter.Consumer).Length);

                    string mtdName = mds.ToNameConvention();
                    if (mtdName.EndsWith("Async"))
                        mtdName = mtdName.Substring(0, mtdName.Length - 5);
                    builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
                    builder.Append("\tpublic record");
                    builder.Append($" {recordPrefix}_{mtdName}_{version}(");

                    var ps = mds.ParameterList.Parameters.Select(p => $"\r\n\t\t\t{p.Type} {p.Identifier.ValueText}");
                    builder.Append("\t\t");
                    builder.Append(string.Join(", ", ps));
                    builder.AppendLine($"): {interfaceName}_EntityFamily;");
                    builder.AppendLine();

                    results.Add(new GenInstruction($"{recordPrefix}.{mtdName}.{version}.Entity", builder.ToString()));
                }
            }


            return results.ToArray();
        }

        #endregion // GenerateEntities   

        #region GenerateEntityFamilyContract

        internal static GenInstruction GenerateEntityFamilyContract(
                            Compilation compilation,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

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
                            Compilation compilation,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            var (item, att, symbol, kind, ns, @using) = info;

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
            builder.AppendLine("\t\t\t\tvar operation_ = announcement.Metadata.Operation;");
            builder.AppendLine("\t\t\t\tvar version_ = announcement.Metadata.Version;");

            var versionInfo = att.GetVersionInfo(compilation, info.Kind);
            int j = 0;
            foreach (var method in item.Members)
            {
                if (method is not MethodDeclarationSyntax mds)
                    continue;
                var opVersionInfo = mds.GetOperationVersionInfo(compilation);
                opVersionInfo.Parent = versionInfo;
                var v = opVersionInfo.Version;
                if (versionInfo.MinVersion > v || versionInfo.IgnoreVersion.Contains(v))
                    continue;

                string version = opVersionInfo.ToString();

                string mtdName = mds.ToNameConvention();
                int opVersion = opVersionInfo.Version;
                string recordSuffix = mtdName.EndsWith("Async") ? mtdName.Substring(0, mtdName.Length - 5) : mtdName;
                string fullRecordName = $"{recordPrefix}_{recordSuffix}";

                string nameOfOperetion = mtdName;
                string ifOrElseIf = j++ > 0 ? "else if" : "if";
                builder.AppendLine($"\t\t\t\t{ifOrElseIf}(operation_ == \"{nameOfOperetion}\" &&");
                builder.AppendLine($"\t\t\t\t\t\t version_ == {opVersion} &&");
                builder.AppendLine($"\t\t\t\t\t\t typeof(TCast) == typeof({fullRecordName}_{version}))");
                builder.AppendLine("\t\t\t\t{");
                var prms = mds.ParameterList.Parameters;
                int i = 0;
                foreach (var p in prms)
                {
                    var pName = p.Identifier.ValueText;
                    builder.AppendLine($"\t\t\t\t\tvar p{i} = await consumerPlan.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }
                var ps = Enumerable.Range(0, prms.Count).Select(m => $"p{m}");

                builder.AppendLine($"\t\t\t\t\t\t{interfaceName}_EntityFamily rec = new {fullRecordName}_{version}({string.Join(", ", ps)});");
                builder.AppendLine($"\t\t\t\t\t\treturn ((TCast?)rec, true);");
                builder.AppendLine("\t\t\t\t}");
            }
            if (item.Members.Count == 0)
                builder.AppendLine($"\t\t\t\treturn Task.FromResult<(TCast? value, bool succeed)>((default, false));");
            else
                builder.AppendLine($"\t\t\t\treturn (default, false);");
            builder.AppendLine("\t\t\t}");

            builder.AppendLine("\t\t}");

            return new GenInstruction($"{friendlyName}.EntityMapper", builder.ToString());
        }

        #endregion // GenerateEntityMapper

        #region GenerateEntityMapperExtensions

        internal static GenInstruction GenerateEntityMapperExtensions(
                            Compilation compilation,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

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
