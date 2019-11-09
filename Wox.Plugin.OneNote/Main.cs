﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;
using Wox.Infrastructure;

namespace Wox.Plugin.OneNote99
{
    public class Main : IPlugin
    {
        private string IconImagePath = "Images\\onenote.png";
        private readonly IOneNoteApi _oneNoteApi;
        private OneNoteCache _cache;

        public Main()
        {
            _oneNoteApi = new OneNoteApi();
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
                var score = StringMatcher.FuzzySearch(queryString, entry.Name).Score;
                if (score > 20)
                {
                    var res = CreateResult(entry.Name, entry.Id, entry.Hierarchy, score);
                    results.Add(res);
                }
            }
            

            // FindUsingOneNoteApi(queryString, results);
            return results;
        }

        private void FindUsingOneNoteApi(string queryString, List<Result> results)
        {
            var doc = _oneNoteApi.FindPages(queryString);
            var pagesContainQuery = doc.Descendants().Where(x => GetName(x)?.ToLower().Contains(queryString) ?? false);
            foreach (var xElement in pagesContainQuery)
            {
                var name = GetName(xElement);
                var id = GetId(xElement);
                var h = GetFullNameHierarchy(xElement);
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

        public static string GetId(XElement xElement)
        {
            return xElement.Attribute(OneNoteApi.IDAttribute)?.Value;
        }

        public static string GetName(XElement element) => element.Attribute(OneNoteApi.NameAttribute)?.Value;

        public static string GetFullNameHierarchy(XElement xElement)
        {
            var name = "";
            xElement = xElement?.Parent;
            while (xElement != null)
            {
                name = GetName(xElement) + '\\' + name;
                xElement = xElement.Parent;
            }

            return name.Trim('\\');
        }

        public void Init(PluginInitContext context)
        {
            _cache = new OneNoteCache(_oneNoteApi);
            _cache.ReloadCache();
        }


        
    }
}