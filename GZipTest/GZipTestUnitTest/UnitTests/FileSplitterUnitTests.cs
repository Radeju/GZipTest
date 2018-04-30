using System;
using System.Collections.Generic;
using System.IO;
using GZipTest.Globals;
using GZipTest.Tools;
using GZipTestUnitTest.Common;
using NUnit.Framework;

namespace GZipTestUnitTest.UnitTests
{
    [TestFixture]
    public class FileSplitterUnitTests
    {
        private readonly string folderName = null;
        private readonly string bigXml = "standard.xml";
        private readonly string decompressedFile = "standard_restored.xml";

        [OneTimeSetUp]
        public void CompressorUnitTestsInit()
        {
            CommonTests.SetDirectory();
        }

        [Test]
        public void SplitTest()
        {
            //Arrange
            FileManipulator manipulator = new FileManipulator();

            //Act
            List<FileInfo> list = SplitFiles(manipulator);
        }

        [Test]
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
