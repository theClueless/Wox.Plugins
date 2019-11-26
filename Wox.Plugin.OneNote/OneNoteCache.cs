using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //<one:Page ID="{6DD529C0-762E-018C-0461-B3B60247D129}{1}{E19534990630611588600120138076622292878245531}"
                        //name="Maroun" dateTime="2019-11-07T06:38:56.000Z" lastModifiedTime="2019-11-07T06:38:59.000Z" pageLevel="1" />
        //<one:Page ID = "{6DD529C0-762E-018C-0461-B3B60247D129}{1}{E19540860559378138484120135987021111103018271}"
        //              name="Template Nov19" dateTime="2019-07-25T18:39:30.000Z" lastModifiedTime="2019-11-23T21:57:17.000Z" pageLevel="2" />

        private List<OneNoteEntry> ConvertDocToEntries(XDocument doc)
        {
            var res = new List<OneNoteEntry>();
            var idSet = new Dictionary<string, string>();
            var perLevelDictionary = new Dictionary<int, string>();
            
            foreach (var xElement in doc.Descendants())
            {
                
                var id = OneNoteXmlHelper.GetId(xElement);
                if (id == null)
                {
                    continue;
                }

                var name = OneNoteXmlHelper.GetName(xElement);

                string hierarchy;
                var pageLevel = OneNoteXmlHelper.GetPageLevel(xElement);
                if (pageLevel != null)
                {
                    var val = pageLevel.Value;
                    hierarchy = 
                        val == 1 
                            ? OneNoteXmlHelper.GetFullNameHierarchy(xElement) 
                            : $"{perLevelDictionary[pageLevel.Value - 1]}";
                    var nextLevelHierarchy = $"{hierarchy}\\{name}";
                    perLevelDictionary[pageLevel.Value] = nextLevelHierarchy;
                }
                else
                {
                    hierarchy = OneNoteXmlHelper.GetFullNameHierarchy(xElement);
                }

                if (!idSet.ContainsKey(hierarchy))
                {
                    idSet[hierarchy] = hierarchy;
                }
                else
                {
                    hierarchy = idSet[hierarchy];
                }
                
                var entry = new OneNoteEntry
                {
                    Name = name,
                    Id = id,
                    Hierarchy = hierarchy,
                    PageLevel = pageLevel.GetValueOrDefault()
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