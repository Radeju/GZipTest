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
        private readonly string compressedFileName = "XMLCompressed";
        private readonly string decompressedFilename = "XMLDecompressed";

        [TestMethod]
        public void CompressTest()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fileInfo = new FileInfo(xmlFile);

            //Act
            int result = compressor.Compress(fileInfo, compressedFileName);

            //Assert
            Assert.Equals(result, 0);
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
    }
}
