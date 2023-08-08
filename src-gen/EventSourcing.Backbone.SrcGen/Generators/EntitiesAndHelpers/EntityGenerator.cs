using System.Reflection;
using System.Text;

using EventSourcing.Backbone.SrcGen.Generators.Entities;

using Microsoft.CodeAnalysis;

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
                            AssemblyName assemblyName)
        {
            var builder = new StringBuilder();
            string FAMILY = $"{interfaceName}_EntityFamily";

            var bundles = info.ToBundle(compilation, true);

            var results = new List<GenInstruction>();

            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            foreach (var bundle in bundles)
            {
                IMethodSymbol method = bundle.Method;
                int version = bundle.Version;
                string deprecateAddition = bundle.Deprecated ? "_Deprecated" : string.Empty;

                method.CopyDocumentation(builder, version, "\t\t");
                builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
                builder.Append("\tpublic record");
                builder.Append($" {bundle}{deprecateAddition}(");

                var psRaw = bundle.Method.Parameters;
                var ps = psRaw.Select(p => $"\r\n\t\t\t{p.Type} {p.Name}");

                builder.Append("\t\t");
                builder.Append(string.Join(", ", ps));
                builder.AppendLine($"): {FAMILY}");
                builder.AppendLine("\t{");
                builder.AppendLine($"\t\tprivate static readonly OperationSignature _signature = new (\"{bundle.FullName}\", {bundle.Version}, \"{bundle.Parameters}\");");
                builder.AppendLine($"\t\tpublic static bool IsMatch(IConsumerInterceptionContext context)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tMetadata meta = context.Context.Metadata;");
                builder.AppendLine($"\t\t\treturn meta.Signature != _signature;");
                builder.AppendLine("\t\t}");
                builder.AppendLine();

                var prms = Enumerable.Range(0, psRaw.Length).Select(m => $"p{m}");
                builder.Append($"\t\tpublic ");
                if(psRaw.Length != 0)
                    builder.Append($"async ");
                builder.AppendLine($"static Task<(bool, {bundle}{deprecateAddition}?)> TryGetAsync(IConsumerInterceptionContext context)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tif(IsMatch(context))");
                if (psRaw.Length != 0)
                    builder.AppendLine($"\t\t\t\treturn (false, null);");
                else
                    builder.AppendLine($"\t\t\t\treturn Task.FromResult<(bool, {bundle}{deprecateAddition}?)>((false, null));");
                builder.AppendLine();
                int i = 0;
                foreach (var p in psRaw)
                {
                    var pName = p.Name;
                    builder.AppendLine($"\t\t\tvar p{i} = await context.GetParameterAsync<{p.Type}>(\"{pName}\");");
                    i++;    
                }

                builder.AppendLine($"\t\t\tvar data = new {bundle}{deprecateAddition}({string.Join(", ", prms)});");
                if (psRaw.Length != 0)
                    builder.AppendLine($"\t\t\treturn (true, data);");
                else
                    builder.AppendLine($"\t\t\treturn Task.FromResult<(bool, {bundle}{deprecateAddition}?)>((true, data));");

                builder.AppendLine("\t\t}");
                builder.AppendLine();

                builder.AppendLine($"\t\tpublic static bool IsMatch(Announcement announcement)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tMetadata meta = announcement.Metadata;");
                builder.AppendLine($"\t\t\treturn meta.Signature != _signature;");
                builder.AppendLine("\t\t}");

                builder.Append($"\t\tpublic ");
                if (psRaw.Length != 0)
                    builder.Append($"async ");
                builder.AppendLine($"static Task<(bool, {bundle}{deprecateAddition}?)> TryGetAsync(IConsumerBridge consumerBridge, Announcement announcement)");
                builder.AppendLine("\t\t{");
                builder.AppendLine($"\t\t\tif(IsMatch(announcement))");

                if (psRaw.Length != 0)
                    builder.AppendLine($"\t\t\t\treturn (false, null);");
                else
                    builder.AppendLine($"\t\t\t\treturn Task.FromResult<(bool, {bundle}{deprecateAddition}?)>((false, null));");
                builder.AppendLine();
                i = 0;
                foreach (var p in psRaw)
                {
                    var pName = p.Name;
                    builder.AppendLine($"\t\t\tvar p{i} = await consumerBridge.GetParameterAsync<{p.Type}>(announcement, \"{pName}\");");
                    i++;
                }

                builder.AppendLine($"\t\t\tvar data = new {bundle}{deprecateAddition}({string.Join(", ", prms)});");
                if (psRaw.Length != 0)
                    builder.AppendLine($"\t\t\treturn (true, data);");
                else
                    builder.AppendLine($"\t\t\treturn Task.FromResult<(bool, {bundle}{deprecateAddition}?)>((true, data));");

                builder.AppendLine("\t\t}");

                builder.AppendLine("\t}");
                builder.AppendLine();

            }

            GenerateEntityFamilyContract(builder, friendlyName, info, interfaceName, assemblyName);
            GenerateEntitiesExtensions(builder, bundles, simpleName, interfaceName, assemblyName);

            results.Add(new GenInstruction($"{simpleName}.Entities", builder.ToString(), $"{info.Namespace}.Generated.{simpleName}"));

            return results.ToArray();
        }

        #endregion // GenerateEntities   

        #region GenerateEntitiesExtensions    

        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="bundles">The bundles.</param>
        /// <param name="simpleName">Name of the simple.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        /// File name
        /// </returns>
        internal static void GenerateEntitiesExtensions(
                            StringBuilder builder, 
                            MethodBundle[] bundles,
                            string simpleName,
                            string interfaceName,
                            AssemblyName assemblyName)
        {
            builder.AppendLine($"\t[GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
            builder.AppendLine($"\tpublic static class {simpleName}EntitiesExtensions");
            builder.AppendLine("\t{");
            foreach (var bundle in bundles)
            {
                string deprecateAddition = bundle.Deprecated ? "_Deprecated" : string.Empty;

                builder.AppendLine($"\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Try to get entity of event of");
                builder.AppendLine($"\t\t///   Operation:{bundle.FullName}");
                builder.AppendLine($"\t\t///   Version:{bundle.Version}");
                builder.AppendLine($"\t\t///   Parameters:{bundle.Parameters}");
                builder.AppendLine($"\t\t/// </summary>");
                builder.AppendLine($"\t\tpublic static Task<(bool, {bundle}{deprecateAddition}?)> TryGet{bundle}{deprecateAddition}Async(this IConsumerInterceptionContext context) => {bundle}{deprecateAddition}.TryGetAsync(context);");
                builder.AppendLine();

                builder.AppendLine($"\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Try to get entity of event of");
                builder.AppendLine($"\t\t///   Operation:{bundle.FullName}");
                builder.AppendLine($"\t\t///   Version:{bundle.Version}");
                builder.AppendLine($"\t\t///   Parameters:{bundle.Parameters}");
                builder.AppendLine($"\t\t/// </summary>");
                builder.AppendLine($"\t\tpublic static Task<(bool, {bundle}{deprecateAddition}?)> TryGet{bundle}{deprecateAddition}Async(this IConsumerBridge bridge, Announcement announcement) => {bundle}{deprecateAddition}.TryGetAsync(bridge, announcement);");

                builder.AppendLine($"\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Check if match entity of event of");
                builder.AppendLine($"\t\t///   Operation:{bundle.FullName}");
                builder.AppendLine($"\t\t///   Version:{bundle.Version}");
                builder.AppendLine($"\t\t///   Parameters:{bundle.Parameters}");
                builder.AppendLine($"\t\t/// </summary>");
                builder.AppendLine($"\t\tpublic static bool IsMatch{bundle}{deprecateAddition}(this IConsumerInterceptionContext context) => {bundle}{deprecateAddition}.IsMatch(context);");
                builder.AppendLine();

                builder.AppendLine($"\t\t/// <summary>");
                builder.AppendLine($"\t\t/// Check if match entity of event of");
                builder.AppendLine($"\t\t///   Operation:{bundle.FullName}");
                builder.AppendLine($"\t\t///   Version:{bundle.Version}");
                builder.AppendLine($"\t\t///   Parameters:{bundle.Parameters}");
                builder.AppendLine($"\t\t/// </summary>");
                builder.AppendLine($"\t\tpublic static bool IsMatch{bundle}{deprecateAddition}(this Announcement announcement) => {bundle}{deprecateAddition}.IsMatch(announcement);");
            }

            builder.AppendLine("\t}");
        }

        #endregion // GenerateEntitiesExtensions   

        #region GenerateEntityFamilyContract

        internal static void GenerateEntityFamilyContract(
                            StringBuilder builder,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            AssemblyName assemblyName)
        {
            string FAMILY = $"{interfaceName}_EntityFamily";

            builder.AppendLine("\t/// <summary>");
            builder.AppendLine($"\t/// Marker interface for entity mapper {interfaceName} contract generated from {interfaceName}");
            builder.AppendLine("\t/// </summary>");
            builder.AppendLine($"\tpublic interface {FAMILY}");
            builder.AppendLine("\t{");
            builder.AppendLine("\t}");
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
            string FAMILY = $"{interfaceName}_EntityFamily";
            var builder = new StringBuilder();
            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine($"\t\tusing Generated.{simpleName};");
            builder.AppendLine();
            builder.AppendLine($"\t\tusing Generated;");
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
            builder.AppendLine("\t\t\t\tvar signature_ = announcement.Metadata.Signature;");

            foreach (var bundle in bundles)
            {
                string deprecateAddition = bundle.Deprecated ? "_Deprecated" : string.Empty;
                string fullRecordName = $"{bundle}{deprecateAddition}";

                builder.AppendLine($"\t\t\t\tif (announcement.IsMatch{fullRecordName}() &&");
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

                builder.AppendLine($"\t\t\t\t\t{FAMILY} rec = new {bundle}({string.Join(", ", ps)});");
                builder.AppendLine($"\t\t\t\t\treturn ((TCast?)rec, true);");
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
            string FAMILY = $"{interfaceName}_EntityFamily";
            var builder = new StringBuilder();

            string bridge = $"{friendlyName}EntityMapper";
            string fileName = $"{bridge}Extensions";

            string simpleName = friendlyName;
            if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
                simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);

            builder.AppendLine($"\t\tusing Generated.{simpleName};");
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
