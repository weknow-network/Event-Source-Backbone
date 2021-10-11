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
                                             .Replace("Pub", "Sub");
            }
            if (kind == "Consumer")
            {
                return txt.Replace("Producer", "Consumer")
                                             .Replace("Subscriber", "Publisher")
                                             .Replace("Sub", "Pub");
            }
            return "ERROR";
        }

        #endregion // Convert
    }
}
