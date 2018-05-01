using System;
using System.Diagnostics;
using GZipTest.Tools;
using GZipTestUnitTest.Common;
using GZipTestUnitTest.UnitTests;
using NUnit.Framework;

namespace GZipTestUnitTest.PerformanceTests
{
    [TestFixture]
    public class PerformanceTests
    {
        private CompressorUnitTests _compressor;

        [OneTimeSetUp]
        public void CompressorUnitTestsInit()
        {
            Common.Common.SetDirectory();
        }

        [SetUp]
        public void CompressorUnitTestsSetUp()
        {
            _compressor = new CompressorUnitTests();
        }

        [Test]
        public void MeasureCompressorTests()
        {
            TimeSpan time = MeasureTime(_compressor.CompressTest);
        }

        [Test]
        public void MeasureCompressorThreadPoolTests()
        {
            TimeSpan time = MeasureTime(_compressor.CompressorThreadPoolTest);
        }

        [Test]
        public void MeasureDecompressingHighMemory()
        {
            TimeSpan time = MeasureTime(_compressor.DecompressConcatenatedStreamsTestHighMemory);
        }

        [Test]
        public void MeasureDocmpressingLowMemory()
        {
            TimeSpan time = MeasureTime(_compressor.DecompressConcatenatedStreamsTestLowMemory);
        }

        private TimeSpan MeasureTime(Action a)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            Console.WriteLine(sw);
            return sw.Elapsed;
        }
    }
}
