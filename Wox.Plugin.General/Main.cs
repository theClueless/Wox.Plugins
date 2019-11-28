using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Wox.Infrastructure;
using Wox.Infrastructure.Logger;

namespace Wox.Plugin.General
{
    public class Main : IPlugin
    {
        public const string CountActionKeyword = "word";

        private readonly IClipboardHelper _clipboardHelper;

        public Main()
        {
            if (_clipboardHelper == null)
            {
                _clipboardHelper = new ClipboardHelper();
            }
        }

        public Main(IClipboardHelper clipboardHelper) : this()
        {
            _clipboardHelper = clipboardHelper;
        }

        public List<Result> Query(Query query)
        {
            var list = new List<Result>();

            var queryString = query.Search.Trim();
            
            if (query.ActionKeyword == CountActionKeyword)
            {
                var stringToCount = queryString;
                bool fromClip = false;
                if (string.IsNullOrWhiteSpace(stringToCount))
                { // try clipboard
                    stringToCount = _clipboardHelper.GetClipboardText();
                    fromClip = true;
                }

                if (!string.IsNullOrEmpty(stringToCount))
                {
                    var result = CreateCountResultFromString(stringToCount, fromClip);
                    list.Add(result);
                }
            }

            if (query.FirstSearch.ToLower() == "logs")
            {
                var logFolder = Path.Combine(Constant.DataDirectory, Log.DirectoryName, Constant.Version);
                if (Directory.Exists(logFolder))
                {
                    var latestLogFile = Directory.GetFiles(logFolder)
                        .Select(x => new FileInfo(x))
                        .OrderByDescending(y => y.LastWriteTimeUtc)
                        .FirstOrDefault();
                    if (latestLogFile != null)
                    {
                        var logsResult = new Result
                            {
                                Title = "Log File",
                                SubTitle = "Open Latest Log File",
                                IcoPath = logFolder,
                                Action = c =>
                                {
                                    Process.Start(latestLogFile.FullName);
                                    return true;
                                }
                            };
                        list.Add(logsResult);
                    }
                }

                
            }

            return list;
        }

        private static Result CreateCountResultFromString(string stringToCount, bool fromClip)
        {
            var maxLength = 50;
            var cut = stringToCount.Substring(0, stringToCount.Length > maxLength ? maxLength : stringToCount.Length).Replace(Environment.NewLine, "#L");
            var title = $"Char Count: {stringToCount.Length}";
            if (fromClip)
            {
                title += " (source - clipboard)";
            }

            var result = new Result
            {
                Score = 99,
                IcoPath = "Images\\count.png",
                Title = title,
                SubTitle = cut
            };
            return result;
        }

        public void Init(PluginInitContext context)
        {
        }
    }


}
