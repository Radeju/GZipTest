using System.Diagnostics;
using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTestUnitTest.UnitTests
{
    [TestClass]
    public class CompressorUnitTests
    {
        private readonly string xmlFile = "XMLTestFile.xml";
        private readonly string bigXmlFile = "standard.xml";
        private readonly string compressedFileName = "XMLCompressed";
        private readonly string decompressedFilename = "XMLDecompressed";

        [TestMethod]
        public void CompressTest()
        {
            //Arrange
            Stopwatch sw = new Stopwatch();
            ICompressor compressor = new Compressor();
            FileInfo fileInfo = new FileInfo(bigXmlFile);

            //Act
            int result = compressor.Compress(fileInfo, compressedFileName);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [TestMethod]
        public void CompressMultiThreadedTest()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fileInfo = new FileInfo(bigXmlFile);

            //Act
            compressor.CompressMultiThread(fileInfo, compressedFileName);
        }

        [TestMethod]
        public void DecompressTest()
        {
            //Arrange
            ICompressor decompressor = new Compressor();
            FileInfo fileInfo = new FileInfo(compressedFileName);

            //Act
            int result = decompressor.Decompress(fileInfo, decompressedFilename);

            //Assert
            Assert.Equals(result, 0);
        }

        [TestMethod]
        public void ThreadPoolTest()
        {
            //Arrange
            ICompressor decompressor = new Compressor();
            FileInfo fileInfo = new FileInfo(bigXmlFile);

            //Act
            ThreadPoolApproach.CompressMultithread(fileInfo, compressedFileName);
        }
    }
}
