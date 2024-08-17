using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Agent.TitleTranslaters
{
    public class ImdbTranslater : ITitleTranslater
    {
        private readonly RestClient _client;

        public ImdbTranslater()
        {
            this._client = new RestClient("https://www.imdb.com");
            this._client.AddDefaultHeader("Accept-Language", "en-US,en;q=0.9;q=0.8");
        }

        public string TranslateTitle(string title)
        {
            var request = new RestRequest("/find/");
            request.AddQueryParameter("q", HttpUtility.UrlEncode(title));
            request.AddQueryParameter("s", "tt");
            request.AddQueryParameter("exact", "true");

            var response = this._client.Get(request);
            var content = response.Content;

            var match = Regex.Match(content, "<td class=\"result_text\"> <a .*?>(.*?)</a>");

            return match.Groups[1].Value;
        }
    }
}
