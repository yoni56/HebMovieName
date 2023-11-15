using System.IO;
using System.Threading;

namespace Agent.Models
{
    public class Movie
    {
        public FileInfo FileInfo { get; set; }
        public CountdownEvent CountdownEvent { get; set; }
    }
}
