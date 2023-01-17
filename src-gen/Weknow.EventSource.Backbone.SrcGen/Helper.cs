using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Helper
    {
        #region Convert

        /// <summary>
        /// Converts the specified text.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <param name="kind">The kind.</param>
        /// <returns></returns>
        internal static string Convert(string txt, string kind)
        {
            if (kind == "Producer")
            {
                return txt.Replace("Consumer", "Producer")
                                             .Replace("Subscriber", "Publisher")
                                             .Replace("Sub", "Pub").Trim();
            }
            if (kind == "Consumer")
            {
                return txt.Replace("Producer", "Consumer")
                                             .Replace("Publisher", "Subscriber")
                                             .Replace("Pub", "Sub").Trim();
            }
            return "ERROR";
        }

        #endregion // Convert

        #region CopyDocumentation

        /// <summary>
        /// Copies the documentation.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="mds">The MDS.</param>
        /// <param name="indent">The indent.</param>
        public static void CopyDocumentation(StringBuilder source, string kind, CSharpSyntaxNode mds, string indent = "\t\t")
        {
            var trivia = mds.GetLeadingTrivia()
                            .Where(t =>
                                    t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                                    t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
            foreach (var doc in trivia)
            {
                source.AppendLine($"{indent}/// {Convert(doc.ToString(), kind)}");
            }
        }

        #endregion // CopyDocumentation

        #region // Hierarchy

        //public static IList<INamedTypeSymbol> Hierarchy(this INamedTypeSymbol symbol)
        //{
        //    var hierarchy = new List<INamedTypeSymbol> { symbol };
        //    var s = symbol.BaseType;
        //    while (s != null && s.Name != "Object")
        //    {
        //        hierarchy.Add(s);
        //        s = s.BaseType;
        //    }
        //    return hierarchy;
        //}

        #endregion // Hierarchy
    }
}
