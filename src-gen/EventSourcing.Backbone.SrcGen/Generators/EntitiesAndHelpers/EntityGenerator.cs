using System.Reflection;
using System.Text;

using EventSourcing.Backbone.SrcGen.Generators.Entities;

using Microsoft.CodeAnalysis;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers
{

    internal static class EntityGenerator
    {
        public const string FAMILY = "IEntityFamily";

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
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();

            var bundles = info.ToBundle(compilation, true);

            var results = new List<GenInstruction>();

            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine($"\tpublic static class {simpleName}");
            builder.AppendLine("\t{");
            foreach (var bundle in bundles)
            {
                IMethodSymbol method = bundle.Method;
                int version = bundle.Version;
                string mtdName = bundle.Name;
                string mtdShortName = mtdName.EndsWith("Async")
                            ? mtdName.Substring(0, mtdName.Length - 5)
                            : mtdName;
                string nameVersion = $"{mtdShortName}_{version}";
                string prmSig = bundle.Parameters;

                method.CopyDocumentation(builder, version, "\t\t");

                builder.Append("\t\tpublic record");
                builder.Append($" {bundle}(");

                var ps = method.Parameters.Select(p => $"\r\n\t\t\t{p.Type} {p.Name}");
                builder.Append("\t\t\t");
                builder.Append(string.Join(", ", ps));
                builder.AppendLine($"): {FAMILY};");
                builder.AppendLine();

            }

            GenerateEntityFamilyContract(builder, friendlyName, info, interfaceName, assemblyName);

            builder.AppendLine("\t}");

            results.Add(new GenInstruction($"{simpleName}.Entities", builder.ToString(), $"{info.Namespace}.Generated.Entities"));

            return results.ToArray();
        }

        #endregion // GenerateEntities   

        #region GenerateEntityFamilyContract

        internal static void GenerateEntityFamilyContract(
                            StringBuilder builder,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            AssemblyName assemblyName)
        {
            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Marker interface for entity mapper FAMILY contract generated from {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\t\tpublic interface {FAMILY}");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t}");
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
            var item = info.Type;
            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine($"\t\tusing static Generated.Entities.{simpleName};");
            builder.AppendLine();
            builder.AppendLine($"\t\tusing Generated.Entities;");
            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Entity mapper is responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t\t[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\t\tpublic sealed class {friendlyName}EntityMapper: IConsumerEntityMapper<{FAMILY}>");
            builder.AppendLine("\t\t{");

            builder.AppendLine("\t\t\t/// <summary>");
            builder.AppendLine($"\t\t\t/// Singleton entity mapper which responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t\t/// </summary>");
            builder.AppendLine($"\t\t\tinternal static readonly IConsumerEntityMapper<{FAMILY}> Default = new {friendlyName}EntityMapper();");


            builder.AppendLine();
            builder.AppendLine($"\t\t\tprivate {friendlyName}EntityMapper() {{}}");
            builder.AppendLine();

            string recordPrefix = friendlyName;
            if (recordPrefix.EndsWith(nameof(KindFilter.Consumer)))
                recordPrefix = recordPrefix.Substring(0, recordPrefix.Length - nameof(KindFilter.Consumer).Length);

            var bundles = info.ToBundle(compilation);

            builder.AppendLine("\t\t\t/// <summary>");
            builder.AppendLine($"\t\t\t///  Try to map announcement");
            builder.AppendLine("\t\t\t/// </summary>");
            builder.AppendLine("\t\t\t/// <typeparam name=\"TCast\">Cast target</typeparam>");
            builder.AppendLine("\t\t\t/// <param name=\"announcement\">The announcement.</param>");
            builder.AppendLine("\t\t\t/// <param name=\"consumerPlan\">The consumer plan.</param>");
            if (bundles.Length == 0)
                builder.Append($"\t\t\t public ");
            else
                builder.Append($"\t\t\t public async ");

            builder.AppendLine($"Task<(TCast? value, bool succeed)> TryMapAsync<TCast>(");
            builder.AppendLine($"\t\t\t\t\tAnnouncement announcement, ");
            builder.AppendLine($"\t\t\t\t\tIConsumerPlan consumerPlan)");
            builder.AppendLine($"\t\t\t\t\t\t where TCast : {FAMILY}");
            builder.AppendLine("\t\t\t{");
            builder.AppendLine("\t\t\t\tvar operation_ = announcement.Metadata.Operation;");
            builder.AppendLine("\t\t\t\tvar version_ = announcement.Metadata.Version;");


            int j = 0;
            foreach (var bundle in bundles)
            {
                string fullRecordName = $"{bundle}";

                string nameOfOperetion = bundle.Name;
                string ifOrElseIf = j++ > 0 ? "else if" : "if";
                builder.AppendLine($"\t\t\t\t{ifOrElseIf}(operation_ == \"{nameOfOperetion}\" &&");
                builder.AppendLine($"\t\t\t\t\t\t version_ == {bundle.Version} &&");
                builder.AppendLine($"\t\t\t\t\t\t typeof(TCast) == typeof({fullRecordName}))");
                builder.AppendLine("\t\t\t\t{");
                var prms = bundle.Method.Parameters;
                int i = 0;
                foreach (var p in prms)
                {
                    var pName = p.Name;
                    builder.AppendLine($"\t\t\t\t\tvar p{i} = await consumerPlan.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }
                var ps = Enumerable.Range(0, prms.Length).Select(m => $"p{m}");

                builder.AppendLine($"\t\t\t\t\t\t{FAMILY} rec = new {bundle}({string.Join(", ", ps)});");
                builder.AppendLine($"\t\t\t\t\t\treturn ((TCast?)rec, true);");
                builder.AppendLine("\t\t\t\t}");
            }
            if (bundles.Length == 0)
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

            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine($"\t\tusing static Generated.Entities.{simpleName};");
            builder.AppendLine();
            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Entity mapper is responsible of mapping announcement to DTO generated from {friendlyName}");
            builder.AppendLine("\t\t/// </summary>");
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic static class {fileName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\t/// <summary>");
            builder.AppendLine($"\t\t/// Specialize Enumerator of event produced by {interfaceName}");
            builder.AppendLine("\t\t/// </summary>");

            builder.AppendLine($"\t\tpublic static IConsumerIterator<{FAMILY}> Specialize{friendlyName} (this IConsumerIterator iterator)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn iterator.Specialize({bridge}.Default);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t}");

            return new GenInstruction($"{friendlyName}.EntityMapper.Extensions", builder.ToString());
        }

        #endregion // GenerateEntityMapperExtensions
    }
}
