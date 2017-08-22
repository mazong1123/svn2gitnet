using System;
using System.Text.RegularExpressions;

namespace Svn2GitNet
{
    public static class Utils
    {
        public static string EscapeQuotes(string source)
        {
            return Regex.Replace(source, "'|\"", (Match m) =>
            {
                return "\\" + m.Value;
            });
        }

        public static string RemoveFromTwoEnds(string source, char pattern)
        {
            char[] trimChars = { pattern };
            return source.TrimEnd(trimChars).TrimStart(trimChars);
        }
    }
}