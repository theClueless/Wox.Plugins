using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin.OneNote99;
using Microsoft.Office.Interop.OneNote;
using Moq;
using Wox.Plugin.Test.Common;

namespace Wox.Plugin.Onenote.Test
{
    [TestClass]
    public class MainFixture
    {
        [TestMethod]
        public void QueryTest_SimpleSuccessfull()
        {
            // arrange
            var doc = XDocument.Parse(Properties.Resources.BasicResult);
            var apiMock = Mock.Of<IOneNoteApi>(x => x.GetAllPages() == doc);
            var main = new Main(apiMock);

            // act
            main.Init(null);
            var res = main.Query(QueryBuilder.Create("Untitled"));

            // assert
            Assert.IsTrue(res.Count == 1);
            var result = res.OrderByDescending(x=>x.Score).First();

            Assert.AreEqual("Untitled page", result.Title);
            Assert.AreEqual("notebook1\\Section 1", result.SubTitle);

            var actionRes = result.Action.Invoke(new ActionContext());
            Mock.Get(apiMock).Verify(x => x.NavigateTo(It.IsAny<string>()), Times.Once());
        }


        // new Query("")
        // var t = main.Query("today");
        //var onApplication = new Application();
        //onApplication.GetHierarchy(null, HierarchyScope.hsPages, out string strXML);
        // var doc = new XDocument(strXML);
        // Clipboard.SetText(strXML)

        // [TestMethod]
        public void QueryTest_SimpleSuccessfull2()
        {
            OneNoteApi api = new OneNoteApi();
            var pages2 = api.GetAllPages();
            var pages = api.FindPages("today");
            XName a = XName.Get("Page", pages.Root.Name.NamespaceName);
            var page = pages.Descendants(a).ToList();

        }

        
    }
}
