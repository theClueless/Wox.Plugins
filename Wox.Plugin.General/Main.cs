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
        public const string LogsKeyword = "logs";
        public const string DataFolderKeyword = "settings";
        public const string MemoryKeyword = "mem";

        private readonly IClipboardHelper _clipboardHelper;
        private readonly ILanguageFixerHandler _languageFixerHandler;
        private IPublicAPI _publicApi;

        public Main()
        {
            if (_clipboardHelper == null)
            {
                _clipboardHelper = new ClipboardHelper();
            }

            if (_languageFixerHandler == null)
            {
                _languageFixerHandler = new LanguageFixerHandler();
            }
        }

        public Main(IClipboardHelper clipboardHelper, ILanguageFixerHandler fixerHandler) : this()
        {
            _clipboardHelper = clipboardHelper;
            _languageFixerHandler = fixerHandler;
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

            switch (query.FirstSearch.ToLower())
            {
                case LogsKeyword:
                    LogsCommandHandler(list);
                    break;
                case MemoryKeyword:
                    MemoryCommandHandler(list, _publicApi);
                    break;
                case DataFolderKeyword:
                    DataFolderCommandHandler(list);
                    break;


            }

            TryFixLanguage(query, list);

            return list;
        }

        private void MemoryCommandHandler(List<Result> list, IPublicAPI publicApi)
        {
            var process = Process.GetCurrentProcess();
            process.Refresh();
            var memorySizeInMB = (process.WorkingSet64 / 1024) / 1024;
            var result = new Result
            {
                Score = 200,
                Title = $"Memory: {memorySizeInMB} MB",
                Action = c =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    publicApi.ChangeQuery(MemoryKeyword, true);
                    return false;
                }

            };

            list.Add(result);
        }

        private void TryFixLanguage(Query query, List<Result> list)
        {
            var res = _languageFixerHandler.TryFix(query, _publicApi);
            if (res != null)
            {
                list.Add(res);
            }
        }

        private static void DataFolderCommandHandler(List<Result> list)
        {
            var dataDirectory = Constant.DataDirectory;
            if (Directory.Exists(dataDirectory))
            {
                var logsResult = new Result
                {
                    Title = "Wox Data Folder",
                    SubTitle = dataDirectory,
                    IcoPath = dataDirectory,
                    Action = c =>
                    {
                        Process.Start(dataDirectory);
                        return true;
                    },
                    Score = 120
                };
                list.Add(logsResult);
            }
        }

        private static void LogsCommandHandler(List<Result> list)
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
            _publicApi = context.API;
        }
    }
}
