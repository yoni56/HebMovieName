namespace Agent.Extensions
{
    public static class StringExt
    {
        public static string LowerClean(this string str)
        {
            return str
                .Replace(",", "")
                .Replace(":", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace(" ", "")
                .ToLower();
        }
    }
}
