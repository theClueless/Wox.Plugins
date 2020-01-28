using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Wox.Infrastructure;
using Wox.Infrastructure.Logger;
using Wox.Plugin.OneNote99;

namespace Wox.Plugin.OneNote
{
    public class Main : IPlugin, IReloadable
    {
        private string IconImagePath = "Images\\onenote.png";
        private IOneNoteApi _oneNoteApi;
        private OneNoteCache _cache;

        public Main()
        {
            CreateApi();
        }

        private void CreateApi()
        {
            try
            {
                _oneNoteApi = new OneNoteApi();
            }
            catch (Exception e)
            {
                LogException("Fail to create onenote API", e);
            }
        }

        public Main(IOneNoteApi api)
        {
            _oneNoteApi = api;
        }

        // use interop api to query elements
        // https://docs.microsoft.com/en-us/office/client-developer/onenote/window-interfaces-onenote
        // https://stackoverflow.com/questions/27294510/how-to-write-to-a-onenote-2013-page-using-c-sharp-and-the-onenote-interop

        // run url to open pages - 
        // onenote:///C:\Users\Documents\OneNote%20Notebooks\General\Work%20general.one#Today
        // onenote:///C:\Users\Documents\OneNote%20Notebooks\General\Work%20general.one#Today&section-id={224F2718-4DBF-4E19-9A5F-6FA90E5E2C50}&page-id={D8B3A806-674D-44CE-8A3E-24BF3F048B8C}&end

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            if (_oneNoteApi == null)
            {
                results.Add(new Result
                {
                    Title = "Recreate API",
                    Action = c =>
                    {
                        this.ReCreateApi();
                        return true;
                    }
                });

                return results;
            }

            var queryString = query.Search;

            if (queryString == "")
            {
                results.Add(new Result
                    {
                        Title = "Reload Cache",
                        Action = c =>
                        {
                            _cache.ReloadCache();
                            return true;
                        }
                    });
                
                return results;
            }

            foreach (var entry in _cache.GetCache())
            {
                var score = StringMatcher.FuzzySearch(queryString, entry.Name).Score + 10;
                var score2 = StringMatcher.FuzzySearch(queryString, entry.FullName).Score;
                var totalScore = Math.Max(score2, score);
                if (totalScore > 20)
                {
                    var res = CreateResult(entry.Name, entry.Id, entry.Hierarchy, score);
                    results.Add(res);
                }
            }
            
            return results;
        }

        private void FindUsingOneNoteApi(string queryString, List<Result> results)
        {
            var doc = _oneNoteApi.FindPages(queryString);
            var pagesContainQuery = doc.Descendants().Where(x => OneNoteXmlHelper.GetName(x)?.ToLower().Contains(queryString) ?? false);
            foreach (var xElement in pagesContainQuery)
            {
                var name = OneNoteXmlHelper.GetName(xElement);
                var id = OneNoteXmlHelper.GetId(xElement);
                var h = OneNoteXmlHelper.GetFullNameHierarchy(xElement);
                var score = StringMatcher.FuzzySearch(queryString, name).Score;
                var res = CreateResult(name, id, h, score);
                results.Add(res);
            }
        }

        private Result CreateResult(string name, string id, string hierarchy, int score)
        {
            return new Result
            {
                Score = score,
                Title = name,
                SubTitle = hierarchy,
                IcoPath = IconImagePath,
                Action = context =>
                {
                    _oneNoteApi.NavigateTo(id);
                    return true;
                }
            };
        }

        public void Init(PluginInitContext context)
        {
            if (_oneNoteApi == null)
            {
                return;
            }

            CreateCache();
        }

        private void CreateCache()
        {
            _cache = new OneNoteCache(_oneNoteApi);
            _cache.ReloadCache();
        }

        private void ReCreateApi()
        {
            CreateApi();
            if (_oneNoteApi != null)
            {
                CreateCache();
            }
        }

        public void ReloadData()
        {
            _cache.ReloadCache();
        }

        public static void LogException(string message, Exception e)
        {
            Log.Exception($"|OneNotePlugin|{message ?? "No Message"}", e);
        }
    }
}
