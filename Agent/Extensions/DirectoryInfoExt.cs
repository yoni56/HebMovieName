using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Agent.Extensions
{
    public static class DirectoryInfoExt
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo di, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");

            var files = di.EnumerateFiles();

            return files.Where(file => extensions.Contains(file.Extension));
        }
    }
}
