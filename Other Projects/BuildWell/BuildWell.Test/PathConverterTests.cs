using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;

namespace BuildWell.Test
{
    [TestClass]
    public class PathConverterTests
    {
        [TestMethod]
        public void ConvertUrlToPath()
        {
            string baseUrl = "http://sourcecontrol/svn/Gis/Development/";
            string workspace = @"C:\Users\tkoehler\Desktop\workspace";
            string expected = Path.Combine(workspace, "this\\is\\a\\test\\test.build");

            string actual = PathConverter.ConvertUrlToPath(baseUrl + "this/is/a/test/test.build", baseUrl, workspace);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertPathToUrl()
        {
            string baseUrl = "http://sourcecontrol/svn/Gis/Development/";
            string workspace = @"C:\Users\tkoehler\Desktop\workspace";
            string expected = "http://sourcecontrol/svn/Gis/Development/this/is/a/test";

            string actual = PathConverter.ConvertPathToUrl(@"C:\Users\tkoehler\Desktop\workspace\this\is\a\test", baseUrl, workspace);

            Assert.AreEqual(expected, actual);
        }
    }
}
