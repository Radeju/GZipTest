﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using GZipTest.Globals;
using GZipTest.Interfaces;

namespace GZipTest.Tools.Compressors
{
    public class CompressorMultiThreadLowMemory : CompressorMultiThread, IDecompressConcatenatedStreams
    {
        #region public methods

        /// <summary>
        /// Provides a workaround to decompressing gzip files that are concatenated
        /// I used http://www.zlib.org/rfc-gzip.html for header specification of GZip.
        ///  This class is using streams to reduce load on memory (differently to CompressorMultiThreadHighMemory)
        /// </summary>
        /// <param name="filePath">FileInfo of gzip concatenated file</param>
        /// <param name="decompressedFileName">Name of the decompressed file</param>
        /// <param name="deleteOriginal">Bool flag whether to remove the original file</param>
        /// <returns>The decompressed byte content of the gzip file</returns>
        public int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false)
        {
            List<long> startIndexes = new List<long>();
            ConcurrentBuffer concBuffer = new ConcurrentBuffer(Const.CHUNK_SIZE_IN_MGBS * 1024 * 1024);
            using (FileStream inFileStream = File.OpenRead(filePath.Name))
            {
                int bytesRead = 0;
                int bufferReadCount = 0;
                int byteCount = concBuffer.MaxCount;
                int offset = 0;
                while ((bytesRead = inFileStream.Read(concBuffer.Buffer, offset, byteCount))
                       != 0)
                {
                    int traversableLength = bytesRead - _startOfFilePattern.Length;
                    FindMatches(startIndexes, _startOfFilePattern, concBuffer.Buffer, traversableLength, bufferReadCount, byteCount);

                    //important piece - make sure that pattern split across two buffers is not lost
                    if (bytesRead == byteCount)
                    {
                        concBuffer.MoveLastBytesToBeginning(_startOfFilePattern.Length);
                    }
                    //needed to wrap the last couple of bytes to the next buffer
                    if (bufferReadCount == 0)
                    {
                        byteCount -= _startOfFilePattern.Length;
                        offset += _startOfFilePattern.Length;
                    }
                    bufferReadCount += 1;
                }
                startIndexes.Add(filePath.Length);
            }

            int result = ConcatenateDecompressedChunksLowMemory(startIndexes, filePath, decompressedFileName);

            if (deleteOriginal)
            {
                filePath.Delete();
            }

            return result;
        }

        #endregion public methods endregion

        #region private methods

        private int ConcatenateDecompressedChunksLowMemory(List<long> startIndexes, FileInfo file, string decompressedFileName)
        {
            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (FileStream outFileStream = File.Create(decompressedFileName))
            {
                using (FileStream inFileStream = File.OpenRead(file.Name))
                {
                    for (int i = 0; i < startIndexes.Count - 1; i++)
                    {
                        byte[] chunk = CreateChunksFromStream(startIndexes[i], startIndexes[i+1], inFileStream);

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
                }

                return 0;
            }
        }

        private byte[] CreateChunksFromStream(long startIndex, long endIndex, FileStream inFileStream)
        {
            long length = endIndex - startIndex;
            byte[] chunk = new byte[length];
            inFileStream.Read(chunk, 0, chunk.Length);
            return chunk;
        }

        #endregion private methods endregion
    }
}
