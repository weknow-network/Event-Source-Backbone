using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// 
    /// </summary>
    public static class Helper
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

    }
}
