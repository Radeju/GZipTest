using System.IO;
using GZipTest.Interfaces;

namespace GZipTest.Tools
{
    class Compressor : ICompressor
    {
        public int Compress(FileInfo fileToCompress, string archiveName)
        {
            using(FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) 
                    != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream outFileStream = File.Create(archiveName + ".gz"))
                    {
                        
                    }
                }
            }

            return 1;
        }

        public int Decompress(FileInfo fileToDecompress, string decompressedFileName)
        {

            return 1;
        }
    }
}
