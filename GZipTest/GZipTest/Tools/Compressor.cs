using System;
using System.IO;
using GZipTest.Interfaces;
using System.IO.Compression;

namespace GZipTest.Tools
{
    public class Compressor : ICompressor
    {
        private const int BUFFER_SIZE = 16 * 1024;

        public int Compress(FileInfo fileToCompress, string archiveName)
        {
            if (fileToCompress == null || archiveName == null)
            {
                return 1;
            }

            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream outFileStream = File.Create(archiveName + ".gz"))
                    {
                        using (GZipStream compress = new GZipStream(outFileStream, CompressionMode.Compress))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;
                            while ((bytesRead = originalFileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                compress.Write(buffer, 0, bytesRead);

                                //compress.BeginWrite(buffer, 0, buffer.Length, ar => { }, new object());
                            }

                            return 0;
                        }
                    }
                }

                return 1;
            }
        }

        public int Decompress(FileInfo fileToDecompress, string decompressedFileName)
        {
            if (fileToDecompress == null || decompressedFileName == null)
            {
                return 1;
            }

            using (FileStream inFile = fileToDecompress.OpenRead())
            {
                using (FileStream outFile = File.Create(decompressedFileName))
                {
                    using (GZipStream decompress = new GZipStream(inFile, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        while ((bytesRead = decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            //outFile.Write(buffer, 0, bytesRead);

                            outFile.BeginWrite(buffer, 0, bytesRead, ar => { }, new Object());
                        }

                        return 0;
                    }
                }
            }
        }
    }
}
