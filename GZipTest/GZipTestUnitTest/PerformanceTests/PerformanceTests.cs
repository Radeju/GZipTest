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
        [OneTimeSetUp]
        public void CompressorUnitTestsInit()
        {
            Common.Common.SetDirectory();
        }

        [Test]
        public void MeasureCompressorTests()
        {
            CompressorUnitTests compressor = new CompressorUnitTests();
            TimeSpan time = MeasureTime(compressor.CompressTest);
        }

        [Test]
        public void MeasureCompressorThreadPoolTests()
        {
            CompressorUnitTests compressor = new CompressorUnitTests();
            TimeSpan time = MeasureTime(compressor.CompressorThreadPoolTest);
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
