using System.Collections.Generic;
using System.IO;
using GZipTest.Globals;
using GZipTest.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace GZipTestUnitTest.UnitTests
{
    [TestClass]
    public class FileSplitterUnitTests
    {
        private readonly string folderName = null;
        private readonly string bigXml = "standard.xml";
        private readonly string decompressedFile = "standard_restored.xml";

        [TestMethod]
        public void SplitTest()
        {
            //Arrange
            FileManipulator manipulator = new FileManipulator();

            //Act
            List<FileInfo> list = SplitFiles(manipulator);
        }

        [TestMethod]
        public void MergeTest()
        {
            //Arrange
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> list = SplitFiles(manipulator);

            //Act
            manipulator.Merge(list, decompressedFile);
        }

        private List<FileInfo> SplitFiles(FileManipulator manipulator)
        {
            int megabyteCount = Const.CHUNK_SIZE_IN_MGBS;
            return manipulator.Split(bigXml, megabyteCount, folderName);
        }
    }
}
