using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Wox.Plugin.General.Test
{
    [TestClass]
    public class LanguageFixerHandlerTest
    {
        [TestMethod]
        public void BasicScenario_ReturnSuccessfully()
        {
            // Arrange
            var cultureInfoFinder = Mock.Of<ICultureInfoFinder>(x=> x.GetCurrentCultureInfo() == new CultureInfo("he-il"));
            var handler = new LanguageFixerHandler(cultureInfoFinder, Mock.Of<IClipboardHelper>());
            var query = new Query("בדיקה", "בדיקה", new string[] { });
            var convertedString = "cshev";

            // Act
            var result = handler.TryFix(query, Mock.Of<IPublicAPI>());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(convertedString, result.ContextData);
        }

        //[TestMethod]
        //public void Basi()
        //{
        //    // Arrange
        //    var cultureInfoFinder = Mock.Of<ICultureInfoFinder>(x => x.GetCurrentCultureInfo() == new CultureInfo("he"));
        //    var handler = new HebrewFixerHandler(null, Mock.Of<IClipboardHelper>());
        //    var query = new Query("בדיקה", "בדיקה", new string[] { });

        //    // Act
        //    var result = handler.HebrewFixer(query, Mock.Of<IPublicAPI>());

        //    // Assert
        //}
    }
}