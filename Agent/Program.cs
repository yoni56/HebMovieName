

using Agent;
using Agent.Extensions;
using Agent.Jobs;
using Agent.Models;
using Agent.TitleTranslaters;
using FluentScheduler;

var files = new DirectoryInfo(
              PCManager.Instance.GetBaseDirectory
          ).GetFilesByExtensions(".mkv", ".avi", ".mp4").ToList();

if (files.Count == 0)
{
    Console.WriteLine($"Enumerate returns {files.Count} result.");

    return;
}

var waitEvent = new CountdownEvent(files.Count);
var units = files
    .Select(x =>
    {
        var data = new MovieData()
        {
            Info = x
        };
    
        return new TranslateJob(
            data, 
            waitEvent, 
            new ImdbTranslater()
        );
    })
    .ToArray();

JobManager.Initialize(units);

waitEvent.Wait();