using System;
using System.IO;

namespace GZipTest.Tools
{
    internal class PathCorrector
    {
        internal string CorrectOutput(string fileName)
        {
            string dirPath = Environment.CurrentDirectory;
            string filePath = dirPath + "\\" + fileName;
            string resultFilePath = filePath;
            int i = 1;
            while (File.Exists(resultFilePath))
            {
                resultFilePath = filePath + "_" + i;
                i += 1;
            }
            return resultFilePath;
        }
    }
}
