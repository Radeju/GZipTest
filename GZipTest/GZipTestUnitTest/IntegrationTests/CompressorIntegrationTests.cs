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
            Common.Common.SetDirectory();
        }

        /// <summary>
        /// Assert that input file is the same as compressed and decompressed file
        /// </summary>
        [Test]
        public void CompressAndDecompress()
        {
            //Arrange
            ICompressor compressor = new Compressor();

            //Act
            TwoInverseActions(compressor.Compress, compressor.Decompress);
        }

        private void TwoInverseActions(Func<FileInfo,string,bool,int> first, Func<FileInfo,string,bool,int> inverseFirst)
        {
            //Arrange
            FileInfo fiToCompress = new FileInfo(bigXmlFile);
            bool removeOriginal = false;
            bool removeCompressed = true;

            //Act
            int firstResult = first(fiToCompress, compressedFileName, removeOriginal);
            FileInfo fiToDecompress = new FileInfo(compressedFileName + ".gz");
            int secondResult = inverseFirst(fiToDecompress, decompressedFilename, removeCompressed);
            FileInfo fiDecompressed = new FileInfo(decompressedFilename);

            //TODO: Assert that compressed-decompressed is the same as original using byte-by-byte comparison
            //TODO: It can be achieved via reading with streams from both files
            //Simplified assert
            Assert.AreEqual(fiDecompressed.Length, fiToCompress.Length);
        }
    }
}
