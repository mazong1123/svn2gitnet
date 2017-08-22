using System;
using System.Text.RegularExpressions;

namespace Svn2GitNet
{
    public static class Utils
    {
        public static string EscapeQuotes(string src)
        {
            return Regex.Replace(src, "'|\"", (Match m) =>
            {
                return "\\" + m.Value;
            });
        }
    }
}