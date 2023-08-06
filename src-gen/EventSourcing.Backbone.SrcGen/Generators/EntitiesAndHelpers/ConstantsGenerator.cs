using System.Reflection;
using System.Text;

using EventSourcing.Backbone.SrcGen.Entities;
using EventSourcing.Backbone.SrcGen.Generators.Entities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EventSourcing.Backbone.SrcGen.Generators.EntitiesAndHelpers;
internal static class ConstantsGenerator
{
    internal static GenInstruction? GenVersionConstants(
                            Compilation compilation,
                            string friendlyName,
                            SyntaxReceiverResult info,
                            string interfaceName,
                            string generateFrom,
                            AssemblyName assemblyName)
    {
#pragma warning disable S1481 // Unused local variables should be removed
        var (type, att, symbol, kind, ns, isProducer, @using) = info;
#pragma warning restore S1481 // Unused local variables should be removed

        if (kind != "Consumer")
            return null;


        string simpleName = friendlyName;
        if (simpleName.EndsWith(nameof(KindFilter.Consumer)))
            simpleName = simpleName.Substring(0, simpleName.Length - nameof(KindFilter.Consumer).Length);


        var builder = new StringBuilder();
        GenVersionConstants(builder);
        return new GenInstruction($"{simpleName}.Constants", builder.ToString(), $"{info.Namespace}.Generated");

        void GenVersionConstants(StringBuilder builder)
        {
            MethodBundle[] bundles = info.ToBundle(compilation, true);

            string indent = "\t";
            builder.AppendLine($"{indent}partial class {simpleName}");
            builder.AppendLine($"{indent}{{");
            indent = $"{indent}\t";

            builder.AppendLine($"{indent}public static class CONSTANTS");
            builder.AppendLine($"{indent}{{");
            indent = $"{indent}\t";

            builder.AppendLine($"{indent}public static class ACTIVE");
            builder.AppendLine($"{indent}{{");

            var active = bundles.Where(b => !b.Deprecated);
            GenConstantsOperations(active, indent);
            builder.AppendLine($"{indent}}}");

            builder.AppendLine($"{indent}public static class DEPRECATED");
            builder.AppendLine($"{indent}{{");
            var deprecated = bundles.Where(b => b.Deprecated).ToArray();
            GenConstantsOperations(deprecated, indent);
            builder.AppendLine($"{indent}}}");

            void GenConstantsOperations(IEnumerable<MethodBundle> bundles, string indent)
            {

                indent = $"{indent}\t";
                var groupd = bundles.GroupBy(b => b.FullName);
                foreach (var group in groupd)
                {
                    builder.AppendLine($"{indent}public static class {group.Key}");
                    builder.AppendLine($"{indent}{{");

                    indent = $"{indent}\t";
                    var versions = group.GroupBy(b => b.Version);
                    foreach (var version in versions)
                    {
                        builder.AppendLine($"{indent}public class V{version.Key}");
                        builder.AppendLine($"{indent}{{");

                        indent = $"{indent}\t";
                        foreach (var b in version)
                        {
                            string pName = b.Parameters.Replace(",", "_");
                            builder.AppendLine($"{indent}public const string P_{pName} = \"{b.Parameters}\";");
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
            indent = indent.Substring(1);
            builder.AppendLine($"{indent}}}");
        }
    }
}