using System;
using System.Diagnostics;
using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools;
using GZipTest.Tools.Compressors;
using NUnit.Framework;
using GZipTestUnitTest.Common;

namespace GZipTestUnitTest.UnitTests
{
    [TestFixture]
    public class CompressorUnitTests
    {
        private readonly string smallXmlFile = "XMLTestFile.xml";
        private readonly string bigXmlFile = "standard.xml";
        private readonly string compressedFileName = "XMLCompressed";
        private readonly string decompressedFilename = "XMLDecompressed";

        [OneTimeSetUp]
        public void CompressorUnitTestsInit()
        {
            Common.Common.SetDirectory();
        }

        [Test]
        public void CompressTest()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fileInfo = new FileInfo(bigXmlFile);

            //Act
            int result = compressor.Compress(fileInfo, compressedFileName);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void CompressorThreadPoolTest()
        {
            //Arrange
            FileInfo fileInfo = new FileInfo(bigXmlFile);
            IThreadPoolCompressor ctp = new ThreadPoolCompressor();

            //Act
            int result = ctp.ThreadPoolCompress(fileInfo, compressedFileName);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void DecompressTest()
        {
            //Arrange
            ICompressor decompressor = new Compressor();
            FileInfo fileInfo = new FileInfo(compressedFileName);

            //Act
            int result = decompressor.Decompress(fileInfo, decompressedFilename);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void DecompressConcatenatedStreamsTestLowMemory()
        {
            //Arrange
            IDecompressConcatenatedStreams compressor = new CompressorMultiThreadLowMemory();
            var fileInfo = File.Exists(compressedFileName) ? 
                new FileInfo(compressedFileName) : 
                new FileInfo(compressedFileName + ".gz");

            //Act
            int result = compressor.DecompressConcatenatedStreams(fileInfo, decompressedFilename);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void DecompressConcatenatedStreamsTestHighMemory()
        {
            //Arrange
            IDecompressConcatenatedStreams compressor = new CompressorMultiThreadHighMemory();
            var fileInfo = File.Exists(compressedFileName) ?
                new FileInfo(compressedFileName) :
                new FileInfo(compressedFileName + ".gz");

            //Act
            int result = compressor.DecompressConcatenatedStreams(fileInfo, decompressedFilename);

            //Assert
            Assert.AreEqual(result, 0);
        }
    }
}
