using Agent.Extensions;
using Agent.Models;
using Agent.TitleTranslaters;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var movies = new DirectoryInfo(@".\").GetFilesByExtensions(".mkv", ".avi", ".mp4");

            int moviesCount = movies.Count();

            if (moviesCount == 0) return;

            var isDone = new CountdownEvent(moviesCount);

            foreach (FileInfo movie in movies)
            {
                var MovieDS = new Movie
                {
                    FileInfo = movie,
                    CountdownEvent = isDone
                };

                ThreadPool.QueueUserWorkItem((o) => doWork(MovieDS));
            }

            isDone.Wait();
        }

        static void doWork(object o)
        {
            Movie MovieDS = o as Movie;

            string titleShort;

            if (!TryGetMovieTitle(MovieDS.FileInfo.Name, out titleShort))
            {
                MovieDS.CountdownEvent.Signal();
                return;
            }

            string titlePost = FormatTitleForPost(titleShort);

            // if the movie name is longer than 3 words, then we search only for the first three words 
            // (this mechanism is because of fail search results due to characters such as ',' and ':'
            // that are not included in the file name)

            var s = titlePost.Split('+');

            if (s.Length > 3)
            {
                titlePost = String.Join("+", s, 0, 3);
            }

            string html = GetMovieHtmlDecoded(titlePost);

            if (Regex.IsMatch(html, "אין תוצאות"))
            {
                // If we got here, most chances the movie name is in forgein language,
                // so we try to get its english name from imdb (or a different service 
                // that implements ITitleTranslater).

                ITitleTranslater translater = new ImdbTranslater();

                var newTitle = translater.TranslateTitle(titleShort);

                titleShort = newTitle;

                // Make title ready for post
                newTitle = newTitle.Replace(" ", "+");

                html = GetMovieHtmlDecoded(newTitle);
            }

            TranslateFileName(html, MovieDS.FileInfo.Name, titleShort);

            MovieDS.CountdownEvent.Signal();
        }

        static string FormatTitleForPost(string title)
        {
            return title.Replace('-', '+').Replace('.', '+').TrimEnd();
        }

        static bool TryGetMovieTitle(string movieTitle, out string title)
        {
            Match ret = Regex.Match(movieTitle, "(.*?)\\.?-?[0-9]{4}");
            title = ret.Groups[1].Value;
            return ret.Success;
        }

        static void TranslateFileName(string html, string titleFull, string titleShort)
        {
            string pattern = "<h4>(.*?)</h4>.*?<h4>(.*?)</h4>.*?<p>(.*?)</p>";

            var movies = Regex.Matches(html.Replace("\n", ""), pattern, RegexOptions.IgnorePatternWhitespace);

            titleShort = titleShort.LowerClean();

            foreach (Match movie in movies)
            {
                string titleRemote = movie.Groups[2].Value.LowerClean();

                if (titleRemote.Equals(titleShort))
                {
                    string fileExtension = Path.GetExtension(titleFull);

                    string hebrewTitle = movie.Groups[1].Value.TrimEnd();

                    File.Move(titleFull, hebrewTitle + fileExtension);

                    //Save movie description
                    //File.WriteAllText($"{hebrewTitle}.txt", movie.Groups[3].Value);

                    break;
                }
            }
        }

        static string GetMovieHtmlDecoded(string titlePost)
        {
            using (var webClient = new WebClient())
            {
                string formData = $"q={titlePost}&index=0&limit=10";

                webClient.Headers.Add
                (
                    HttpRequestHeader.ContentType,
                    "application/x-www-form-urlencoded; charset=UTF-8"
                );

                string html = webClient.UploadString("https://moridimtv.com/ajax/search.php", formData);

                return HttpUtility.HtmlDecode(html);
            }
        }
    }
}
