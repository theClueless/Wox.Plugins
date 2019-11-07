using System;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wox.Plugin;
using Wox.Plugin.Test.Common;

namespace Wox.WhatupPlugin.Test
{
    [TestClass]
    public class MainTest
    {
        [TestMethod]
        [DataRow("051-146-8976", "0511468976")]
        [DataRow("+051-146-8976", "0511468976")]
        [DataRow("972-51-146-8976", "0511468976")]
        [DataRow("+972-51-146-8976", "0511468976")]
        [DataRow("034540 something +972-51-146-8976", "0511468976")]
        [DataRow("asdasd-as146dasdas-8976", null)]
        [DataRow(@"https://something223.asdfs/phone=972-51-146-8975", "0511468975")]
        public void TryExtractPhoneNumber_ReturnsSuccesfully(string numberString, string expected)
        {
            // act
            var number = Main.TryExtractPhoneNumber(numberString);

            // assert
            Assert.AreEqual(expected, number);
        }

        [TestMethod]
        public void Search_worksWithClipboard()
        {
            var clipboardHelper = Mock.Of<IClipboardHelper>(x=>x.GetClipboardText() == "052-333-2211");
            var main = new Main(clipboardHelper);

            var query = QueryBuilder.Create("", Main.PluginActionKeyworkd);
            var expecetedTitle = "0523332211";
            var res = main.Query(query);

            Assert.IsTrue(res.Count == 1);
            Assert.AreEqual(expecetedTitle, res[0].Title);
            
        }

        [TestMethod]
        public void Search_ReutrnsSucessfully()
        {
            var clipboardHelper = Mock.Of<IClipboardHelper>();
            Mock.Get(clipboardHelper).Setup(x => x.GetClipboardText()).Throws<Exception>();
            var main = new Main(clipboardHelper);

            var query = QueryBuilder.Create("052-333-2211", Main.PluginActionKeyworkd);
            // var query = new Query(Main.PluginActionKeyworkd, "", new string[] { }, Main.PluginActionKeyworkd);
            var expecetedTitle = "0523332211";
            var res = main.Query(query);

            Assert.IsTrue(res.Count == 1);
            Assert.AreEqual(expecetedTitle, res[0].Title);

        }
    }

  
}
