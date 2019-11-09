using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Wox.Plugin.OneNote99
{
    public class OneNoteCache : IDisposable
    {
        private readonly IOneNoteApi _api;
        private List<OneNoteEntry> _cache;

        public OneNoteCache(IOneNoteApi api)
        {
            _api = api;
            if (_api.IsTrackingChanges)
            {
                _api.OneNoteDataChanged += ApiOnOneNoteDataChanged;
            }
        }

        private void ApiOnOneNoteDataChanged(object sender, EventArgs e)
        {
            ReloadCache();
        }

        public void ReloadCache()
        {
            var all = _api.GetAllPages();
            _cache = ConvertDocToEntries(all);
        }

        private List<OneNoteEntry> ConvertDocToEntries(XDocument doc)
        {
            var res = new List<OneNoteEntry>();
            var idSet = new Dictionary<string, string>();
            foreach (var xElement in doc.Descendants())
            {
                var hierarchy = Main.GetFullNameHierarchy(xElement);
                if (!idSet.ContainsKey(hierarchy))
                {
                    idSet[hierarchy] = hierarchy;
                }
                else
                {
                    hierarchy = idSet[hierarchy];
                }

                var id = Main.GetId(xElement);
                if (id == null)
                {
                    continue;
                }

                var entry = new OneNoteEntry
                {
                    Name = Main.GetName(xElement),
                    Id = id,
                    Hierarchy = hierarchy
                };
                res.Add(entry);
            }

            return res;
        }

        public List<OneNoteEntry> GetCache()
        {
            return _cache;
        }

        public void Dispose()
        {
            if (_api.IsTrackingChanges)
            {
                _api.OneNoteDataChanged -= ApiOnOneNoteDataChanged;
            }
        }
    }
}