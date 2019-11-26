using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wox.Plugin.OneNote99;
using Wox.Plugin.Test.Common;

namespace Wox.Plugin.Onenote.Test
{
    [TestClass]
    public class OneNoteCacheFixture
    {
        [TestMethod]
        public void QueryTest_SimpleSuccessfull()
        {
            
            // arrange
            var doc = XDocument.Parse(Properties.Resources.BasicResult);
            var apiMock = Mock.Of<IOneNoteApi>(x => x.GetAllPages() == doc);
            var cache = new OneNoteCache(apiMock);

            // act
            cache.ReloadCache();
            var cacheEntries = cache.GetCache();

            // assert
            Assert.IsTrue(cacheEntries.Count == 7);
            var subPageEntry = cacheEntries.First(x=>x.Name == "Checking subPage");

            Assert.AreEqual(2, subPageEntry.PageLevel);
            Assert.AreEqual("notebook1\\Section 2\\Checking", subPageEntry.Hierarchy);
        }
    }
}