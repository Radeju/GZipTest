using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTestUnitTest.IntegrationTests
{
    [TestClass]
    class CompressorIntegrationTests
    {
        private readonly string xmlFile = "XMLTestFile.xml";
        private readonly string compressedFileName = "XMLCompressed";
        private readonly string decompressedFilename = "XMLDecompressed";

        /// <summary>
        /// Assert that input file is the same as compressed and decompressed file
        /// </summary>
        [TestMethod]
        public void CompressAndDecompress()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fiToCompress = new FileInfo(xmlFile);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);

            //Act
            compressor.Compress(fiToCompress, compressedFileName);
            compressor.Decompress(fiToDecompress, decompressedFilename);
            
            //Assert that compressed-decompressed is the same as original
        }
    }
}
