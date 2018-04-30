using System;
using System.IO;
using GZipTestUnitTest.UnitTests;

namespace GZipTestUnitTest.Common
{
    static class CommonTests
    {
        public static void SetDirectory()
        {
            string dir = Path.GetDirectoryName(typeof(CompressorUnitTests).Assembly.Location);
            Environment.CurrentDirectory = dir;
        }
    }
}
