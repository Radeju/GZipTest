using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using GZipTest.Globals;

namespace GZipTest.Tools.Compressors
{
    public class CompressorMultiThreadHighMemory : CompressorMultiThread
    {
        public CompressorMultiThreadHighMemory
            (ManualResetEvent doneEvent, FileInfo fileToCompress, string archiveName, bool deleteOriginal = false) :
            base(doneEvent, fileToCompress, archiveName, deleteOriginal)
        {

        }

        public override int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName, bool deleteOriginal = false)
        {
            List<long> startIndexes = new List<long>();

            //Get the bytes of the file
            byte[] fileBytes = File.ReadAllBytes(filePath.Name);
            int traversableLength = fileBytes.Length - _startOfFilePattern.Length;
            FindMatches(startIndexes, _startOfFilePattern, fileBytes, traversableLength);

            //Start from the beginning if you did not find anything
            if (!startIndexes.Any())
            {
                startIndexes.Add(0);
            }

            List<byte[]> chunks = CreateChunksFromFile(startIndexes, fileBytes);
            if (deleteOriginal)
            {
                filePath.Delete();
            }
            return ConcatenateDecompressedChunks(chunks, decompressedFileName);
        }

        private List<byte[]> CreateChunksFromFile(List<long> startIndexes, byte[] fileBytes)
        {
            List<byte[]> chunks = new List<byte[]>();
            for (int i = 0; i < startIndexes.Count; i++)
            {
                long length = 0;

                if (i + 1 == startIndexes.Count)
                {
                    length = fileBytes.Length - startIndexes[i];
                }
                else
                {
                    length = startIndexes[i + 1] - startIndexes[i];
                }

                if (length > 0)
                {
                    chunks.Add(fileBytes.Skip((int)startIndexes[i]).Take((int)length).ToArray());
                }
            }

            return chunks;
        }

        private int ConcatenateDecompressedChunks(List<byte[]> chunks, string decompressedFileName)
        {
            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (FileStream outFileStream = File.Create(decompressedFileName))
            {
                foreach (byte[] chunk in chunks)
                {
                    using (MemoryStream mStream = new MemoryStream(chunk))
                    {
                        using (GZipStream gzStream = new GZipStream(mStream, CompressionMode.Decompress))
                        {
                            int bytesRead = 0;
                            while ((bytesRead = gzStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                outFileStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                return 0;
            }
        }
    }
}
