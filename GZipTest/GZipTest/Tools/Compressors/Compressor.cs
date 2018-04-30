using System;
using System.Collections.Generic;
using System.IO;
using GZipTest.Interfaces;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using GZipTest.Globals;

namespace GZipTest.Tools.Compressors
{
    public class Compressor : ICompressor
    {
        public int Compress(FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
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
                            byte[] buffer = new byte[Const.BUFFER_SIZE];
                            int bytesRead;
                            while ((bytesRead = originalFileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                compress.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                else
                {
                    return 1;
                }
            }

            if (deleteOriginal)
            {
                fileToCompress.Delete();
            }

            return 0;
        }
        
        public int Decompress(FileInfo fileToDecompress, string decompressedFileName, bool deleteOriginal = false)
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
                        byte[] buffer = new byte[Const.BUFFER_SIZE];
                        int bytesRead;
                        while ((bytesRead = decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outFile.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }

            if (deleteOriginal)
            {
                fileToDecompress.Delete();
            }
            return 0;
        }        
    }
}
