using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools.Compressors;
using GZipTestUnitTest.Common;
using NUnit.Framework;

namespace GZipTestUnitTest.IntegrationTests
{
    [TestFixture]
    class CompressorIntegrationTests
    {
        private readonly string xmlFile = "XMLTestFile.xml";
        private readonly string bigXmlFile = "standard.xml";
        private readonly string compressedFileName = "XMLCompressed";
        private readonly string decompressedFilename = "XMLDecompressed";

        [OneTimeSetUp]
        public void CompressorUnitTestsInit()
        {
            CommonTests.SetDirectory();
        }

        /// <summary>
        /// Assert that input file is the same as compressed and decompressed file
        /// </summary>
        [Test]
        public void CompressAndDecompress()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fiToCompress = new FileInfo(xmlFile);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);

            //Act
            compressor.Compress(fiToCompress, compressedFileName);
            compressor.Decompress(fiToDecompress, decompressedFilename);
            
            //TODO: Assert that compressed-decompressed is the same as original
            throw new AssertionException("No Assertion implemented yet");
        }

        [Test]
        public void CompressMultiThreadAndDecompress()
        {
            //Arrange
            ICompressorMultithread compressor = new CompressorMultithread();
            FileInfo fiToCompress = new FileInfo(bigXmlFile);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);

            //Act
            compressor.CompressMultiThread(fiToCompress, compressedFileName);
            compressor.Decompress(fiToDecompress, decompressedFilename);

            //TODO: Assert that compressed-decompressed is the same as original
            throw new AssertionException("No Assertion implemented yet");
        }
    }
}
