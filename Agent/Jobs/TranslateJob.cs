using Agent.Extensions;
using Agent.Models;
using Agent.TitleTranslaters;
using FluentScheduler;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Agent.Jobs
{
    public class TranslateJob : Registry, IJob
    {
        private readonly MovieData _entry;
        private readonly CountdownEvent _event;
        private readonly ITitleTranslater _titleLookup;
        private readonly RestClient _client;

        public TranslateJob(
            MovieData entry, 
            CountdownEvent @event, 
            ITitleTranslater titleLookup)
        {
            this.Schedule(this).ToRunNow();

            this._entry = entry;
            this._event = @event;
            this._titleLookup = titleLookup;
            this._client = new RestClient("https://moridimtv.net");
        }

        public void Execute()
        {
            if (!this._entry.TryGetNameWithYear(out string titleNameOnly))
            {
                this._event.Signal();

                return;
            }

            // if the movie name is longer than 3 words, then we search only for the first three words 
            // this mechanism is because of fail search results due to characters such as ',' and ':' that are not included in the file name
            var searchQueryClean = this._entry.CleanCharactersTitle(titleNameOnly);

            var content = GetMovieWebpageMarkup(searchQueryClean);

            if (Regex.IsMatch(content, "אין תוצאות"))
            {
                // If we got here, most chances the movie name is in forgein language,
                // so we try to get its english name from imdb (or a different service 
                // that implements ITitleTranslater).
                titleNameOnly = this._titleLookup
                    .TranslateTitle(titleNameOnly)
                    .Replace(" ", "+");

                content = GetMovieWebpageMarkup(titleNameOnly);
            }

            ChangeFileName(content, this._entry.Name, titleNameOnly);

            this._event.Signal();
        }

        private string FormatTitleForPost(string title)
        {
            return title
                .Replace('-', '+')
                .Replace('.', '+')
                .TrimEnd();
        }

        static void ChangeFileName(string content, string fileNameOriginal, string titleToCompare)
        {
            var pattern = "<h4>(.*?)</h4>.*?<h4>(.*?)</h4>.*?<p>(.*?)</p>";

            var matches = Regex
                .Matches(
                    content.Replace("\n", ""), 
                    pattern, 
                    RegexOptions.IgnorePatternWhitespace
                );

            foreach (Match match in matches)
            {
                var titleWebpage = match.Groups[2].Value.LowerClean();
                var titleCompare = titleToCompare.LowerClean();

                if (titleWebpage.Equals(titleCompare))
                {
                    var fileExt = Path.GetExtension(fileNameOriginal);

                    var hebtitle = match.Groups[1].Value
                        .Trim()
                        .RemoveInvalidCharacters();

                    File.Move(
                        PCManager.Instance.Combine(fileNameOriginal), 
                        PCManager.Instance.Combine(hebtitle + fileExt)
                    );

                    //Save movie description
                    //File.WriteAllText($"{hebrewTitle}.txt", movie.Groups[3].Value);

                    return;
                }
            }
        }

        private string GetMovieWebpageMarkup(string title)
        {
            // create request
            var request = new RestRequest("/ajax/indexFetch.php");

            // prepare data form
            request.AddParameter("table", "movies");
            request.AddParameter("q", title.ToLower());
            request.AddParameter("index", "0");
            request.AddParameter("limit", "10");
            request.AddParameter("type", "0");

            // execute request
            var response = this._client.Post(request);

            // read content
            var content = response.Content;

            return HttpUtility.HtmlDecode(content);
        }
    }
}
