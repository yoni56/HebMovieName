using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Agent.Models
{
    public class MovieData
    {
        public FileInfo Info { get; set; }

        public string Name => Info.Name;

        public bool TryGetNameWithYear(out string title)
        {
            Match ret = Regex.Match(this.Info.Name, "(.*?)\\.?-?[0-9]{4}");
            title = ret.Groups[1].Value;
            return ret.Success;
        }

        public string CleanCharactersTitle(string title)
        {
            title = Regex.Replace(title, "[\\.\\+]+", " ");

            var split = title.Split(" ");

            bool islenGreater = split.Length > 3;
            if (islenGreater)
            {
                title = String.Join(" ", split, 0, 3);
            }

            return title;
        }
    }
}
