namespace School.Core.Models
{
    internal static class StringExtensions
    {
        public static string Clean(this string s) => s?.Trim('\n', '\t', ' ');
    }
}
