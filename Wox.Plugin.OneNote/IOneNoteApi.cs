using System;
using System.Net.Mime;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;
using Wox.Infrastructure.Logger;

namespace Wox.Plugin.OneNote99
{
    public interface IOneNoteApi :  IDisposable
    {
        XDocument FindPages(string query);
        void NavigateTo(string objectId);
        XDocument GetAllPages();

        bool IsTrackingChanges { get; }

        event EventHandler OneNoteDataChanged;
    }

    public class OneNoteApi : IOneNoteApi
    {
        private readonly bool _shouldTrackOneNote;
        public const string IDAttribute = "ID";
        public const string NameAttribute = "name";

        public event EventHandler OneNoteDataChanged;

        protected virtual void OnOneNoteDataChanged(EventArgs e)
        {
            var handler = OneNoteDataChanged;
            handler?.Invoke(this, e);
        }

        private readonly Application _app;

        public OneNoteApi(bool shouldTrackOneNote = false)
        {
            _shouldTrackOneNote = shouldTrackOneNote;
            _app = new Application();
            if(_shouldTrackOneNote)
            {
                _app.OnHierarchyChange+= oneNoteHierarcyChange;
            }
        }

        private void oneNoteHierarcyChange(string id)
        {
            OnOneNoteDataChanged(new EventArgs());
        }

        public XDocument FindPages(string query)
        {
            _app.FindPages(null, query, out string xml);
            return XDocument.Parse(xml);
        }

        public void NavigateTo(string objectId)
        {
            try
            {
                _app.NavigateTo(objectId);
            }
            catch (Exception e)
            {
                Log.Exception($"Fail to navigate to page: {objectId}", e);
            }
            
        }

        public XDocument GetAllPages()
        {
            try
            {
                _app.GetHierarchy(null, HierarchyScope.hsPages, out string strXML);
                return XDocument.Parse(strXML);
            }
            catch (Exception e)
            {
                Log.Exception("Fail to get all pages", e);
                throw;
            }
        }

        public bool IsTrackingChanges => _shouldTrackOneNote;

        public void Dispose()
        {
            if (_shouldTrackOneNote)
            {
                _app.OnHierarchyChange -= oneNoteHierarcyChange;
            }
        }
    }

    public class OneNoteEntry
    {
        public string Hierarchy { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public int PageLevel { get; set; }

        public string FullName => $"{Hierarchy}\\{Name}";
    }

    // onenote xml result:

    // <one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
    // <one:Notebook name = "My Notebook" nickname="My Notebook" ID="{AFF6CE2D-8188-45C8-9829-6FAC1CB66023}{1}{B0}" path="C:\Users\atepper\Documents\OneNote Notebooks\My Notebook" lastModifiedTime="2019-11-07T10:49:38.000Z" color="#B7C997">
    // <one:Section name = "Quick Notes" ID="{54DADC52-B1C5-02DF-0F93-7E8525D86438}{1}{B0}" path="C:\Users\atepper\Documents\OneNote Notebooks\My Notebook\Quick Notes.one" lastModifiedTime="2019-11-07T10:49:38.000Z" color="#FFD869">
    // <one:Page ID = "{54DADC52-B1C5-02DF-0F93-7E8525D86438}{1}{E19526683310196358117620112389334552583645901}" name="17 - 19 - 1 day + 1 day" dateTime="2018-04-17T12:24:37.000Z" lastModifiedTime="2018-04-22T11:18:56.000Z" pageLevel="1" />
    // <one:Page ID = "{54DADC52-B1C5-02DF-0F93-7E8525D86438}{1}{E19510002027725374359920152779706957078271351}" name="Cloud services source control and CI/CD pipeline" dateTime="2019-01-30T08:29:29.000Z" lastModifiedTime="2019-01-30T14:00:30.000Z" pageLevel="1">
    // <one:Meta name = "OutlookEntryHash" content="Yc?@YdUhOJryd" />
    // </one:Page>
    // <one:Page ID = "{54DADC52-B1C5-02DF-0F93-7E8525D86438}{1}{E19527244850716365595720141522872855520659731}" name="לא קראתי טוב. חשבתי שאתה מתכוון לספרים." dateTime="2019-03-10T21:49:54.000Z" lastModifiedTime="2019-03-10T22:58:13.000Z" pageLevel="1" />
    // </one:Section>
    // </one:Notebook>
}