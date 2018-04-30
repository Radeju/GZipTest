using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools.Compressors;
using GZipTestUnitTest.Common;
using NUnit.Framework;
using System;

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
            
            //Act
            compressor.Compress(fiToCompress, compressedFileName);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);
            compressor.Decompress(fiToDecompress, decompressedFilename);
            FileInfo fiDecompressed = new FileInfo(decompressedFilename);

            //TODO: Assert that compressed-decompressed is the same as original using byte-by-byte comparison
            //TODO: It can be achieved via reading with streams from both files
            //Simplified assert
            Assert.AreEqual(fiDecompressed.Length, fiToCompress.Length);
        }

        [Test]
        public void CompressMultiThreadAndDecompress()
        {
            //Arrange
            ICompressorMultithread compressor = new CompressorMultithread();
            FileInfo fiToCompress = new FileInfo(bigXmlFile);
            

            //Act
            compressor.CompressMultiThread(fiToCompress, compressedFileName);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);
            compressor.DecompressConcatenatedStreams(fiToDecompress, decompressedFilename);
            FileInfo fiDecompressed = new FileInfo(decompressedFilename);

            //TODO: Assert that compressed-decompressed is the same as original using byte-by-byte comparison
            //TODO: It can be achieved via reading with streams from both files
            //Simplified assert
            Assert.AreEqual(fiDecompressed.Length, fiToCompress.Length);
        }

        private void TwoInverseActions(Action<FileInfo,string> first, Action<FileInfo,string> inverseFirst)
        {
            //Arrange
            FileInfo fiToCompress = new FileInfo(bigXmlFile);

            //Act
            first(fiToCompress, compressedFileName);
            FileInfo fiToDecompress = new FileInfo(compressedFileName);
            inverseFirst(fiToDecompress, decompressedFilename);
            FileInfo fiDecompressed = new FileInfo(decompressedFilename);

            //TODO: Assert that compressed-decompressed is the same as original using byte-by-byte comparison
            //TODO: It can be achieved via reading with streams from both files
            //Simplified assert
            Assert.AreEqual(fiDecompressed.Length, fiToCompress.Length);
        }
    }
}
