using System.Net;

namespace School.Core.Models
{
    internal static class StringExtensions
    {
        public static string Clean(this string s) => WebUtility.HtmlDecode(s.Trim('\n', '\t', ' '));
    }
}
