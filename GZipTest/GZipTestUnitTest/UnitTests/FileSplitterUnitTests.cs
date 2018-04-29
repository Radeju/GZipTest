using GZipTest.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace GZipTestUnitTest.UnitTests
{
    [TestClass]
    public class FileSplitterUnitTests
    {
        [TestMethod]
        public void SplitTest()
        {
            //Arrange
            FileSplitter splitter = new FileSplitter();

            //Act
            var list = splitter.Split("standard.xml", 10, "Split");
        }
    }
}
