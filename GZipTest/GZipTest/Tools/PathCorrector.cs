using System;
using System.IO;
using System.Text;

namespace GZipTest.Tools
{
    internal class PathCorrector
    {
        internal FileInfo CorrectOutput(string fileName)
        {
            string path = Environment.CurrentDirectory;
            FileInfo outFileInfo = new FileInfo(path + fileName);
            int i = 1;
            while (outFileInfo != null)
            {
                outFileInfo = new FileInfo(path + fileName + "_" + i); //should be no need for Stringbuilder
            }
            return outFileInfo;
        }
    }
}
