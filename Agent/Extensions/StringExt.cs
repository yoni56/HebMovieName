using System.Text.RegularExpressions;

namespace Agent.Extensions
{
    public static class StringExt
    {
        public static string LowerClean(this string str)
        {
            return Regex
                .Replace(str, "[\\,\\:\\-\\.\\s]+", "")
                .ToLower();
        }

        public static string RemoveInvalidCharacters(this string str)
        {
            var chars = Path
                .GetInvalidFileNameChars()
                .Select(c => $"\\{c}")
                .ToArray();

            var pattern = $"[{string.Join("", chars)}]";

            return Regex.Replace(str, pattern, "");
        }
    }
}
