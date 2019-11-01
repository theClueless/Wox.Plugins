using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wox.Plugin.ProcessKiller.Test
{
    [TestClass]
    public class MainTest
    {
        [TestMethod]
        public void Sanity()
        {
            var main = new Main();
            var t = main.Query(new Query("kill", "kill", new string[] { }));
        }
    }
}
