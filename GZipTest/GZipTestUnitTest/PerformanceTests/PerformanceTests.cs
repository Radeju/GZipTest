using System;
using System.Diagnostics;
using GZipTest.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipTestUnitTest.UnitTests;

namespace GZipTestUnitTest.PerformanceTests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void MeasureCompressorTests()
        {
            CompressorUnitTests compressor = new CompressorUnitTests();
            TimeSpan time = MeasureTime(compressor.CompressTest);
        }

        [TestMethod]
        public void MeasureCompressorMultiThreadTests()
        {
            CompressorUnitTests compressor = new CompressorUnitTests();
            TimeSpan time = MeasureTime(compressor.CompressMultiThreadedTest);
        }

        [TestMethod]
        public void MeasureCompressorThreadPoolTests()
        {
            CompressorUnitTests compressor = new CompressorUnitTests();
            TimeSpan time = MeasureTime(compressor.ThreadPoolTest);
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
