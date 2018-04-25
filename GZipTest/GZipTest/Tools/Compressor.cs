using System.IO;
using GZipTest.Interfaces;
using System.IO.Compression;

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
                        using (GZipStream compress = new GZipStream(outFileStream, CompressionMode.Compress))
                        {
                            byte[] buffer = new byte[4096];
                            int numRead;
                            while ((numRead = originalFileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                compress.Write(buffer, 0, numRead);
                            }
                        }
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
