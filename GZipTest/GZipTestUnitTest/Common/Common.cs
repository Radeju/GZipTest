using System;
using System.IO;
using GZipTestUnitTest.UnitTests;

namespace GZipTestUnitTest.Common
{
    static class Common
    {
        public static void SetDirectory()
        {
            string dir = Path.GetDirectoryName(typeof(CompressorUnitTests).Assembly.Location);
            if (dir != null)
            {
                Environment.CurrentDirectory = dir;
            }
        }
    }
}
