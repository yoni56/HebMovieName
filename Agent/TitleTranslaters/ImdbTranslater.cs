using System.Net;
using System.Text.RegularExpressions;

namespace Agent.TitleTranslaters
{
    public class ImdbTranslater : ITitleTranslater
    {
        public string TranslateTitle(string ForeignTitle)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9;q=0.8");

                ForeignTitle = ForeignTitle.Replace("-", "%20");

                string html = webClient.DownloadString($"https://www.imdb.com/find?q={ForeignTitle}&s=tt&exact=true");

                Match match = Regex.Match(html, "<td class=\"result_text\"> <a .*?>(.*?)</a>");

                return match.Groups[1].Value;
            }
        }
    }
}
