using System;
using System.Diagnostics;
using System.IO;
using GZipTest.Interfaces;
using GZipTest.Tools;
using NUnit.Framework;
using GZipTestUnitTest.Common;

namespace GZipTestUnitTest.UnitTests
{
    [TestFixture]
    public class CompressorUnitTests
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

        [Test]
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

        [Test]
        public void CompressMultiThreadedTest()
        {
            //Arrange
            ICompressor compressor = new Compressor();
            FileInfo fileInfo = new FileInfo(bigXmlFile);

            //Act
            compressor.CompressMultiThread(fileInfo, compressedFileName);
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
            Assert.Equals(result, 0);
        }

        [Test]
        public void CompressorThreadPoolTest()
        {
            //Arrange
            ICompressor decompressor = new Compressor();
            var path = Directory.GetCurrentDirectory();
            FileInfo fileInfo = new FileInfo(bigXmlFile);
            CompressorThreadPool ctp = new CompressorThreadPool();

            //Act
            ctp.CompressMultithread(fileInfo, compressedFileName);
        }
    }
}
