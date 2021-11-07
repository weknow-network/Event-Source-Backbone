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
    public static class Helper
    {
        #region Convert

        internal static string Convert(string txt, string kind)
        {
            if (kind == "Producer")
            {
                return txt.Replace("Consumer", "Producer")
                                             .Replace("Publisher", "Subscriber")
                                             .Replace("Pub", "Sub").Trim();
            }
            if (kind == "Consumer")
            {
                return txt.Replace("Producer", "Consumer")
                                             .Replace("Subscriber", "Publisher")
                                             .Replace("Sub", "Pub").Trim();
            }
            return "ERROR";
        }

        #endregion // Convert


        #region CopyDocumentation

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
