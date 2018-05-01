using System;
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
    public class CompressorMultiThread : Compressor, ICompressorMultithread
    {
        private readonly ManualResetEvent _doneEvent;
        private readonly FileInfo _fileToCompress;
        private readonly string _archiveName;
        private readonly bool _deleteOriginal;
        private readonly byte[] _startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public int Status { get; private set; }

        #region public methods endregion
        public CompressorMultiThread() { }

        public CompressorMultiThread(ManualResetEvent doneEvent, FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
        {
            _doneEvent = doneEvent;
            _fileToCompress = fileToCompress;
            _archiveName = archiveName;
            _deleteOriginal = deleteOriginal;
        }

        public void ThreadPoolCallback(object threadContext)
        {
            int threadIndex = (int)threadContext;
            Status = Compress(_fileToCompress, _archiveName, _deleteOriginal);
            _doneEvent.Set();
        }

        public int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();
            byte[] startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
            const long memoryThreshold = 100 * 1024 * 1024;

            int result = filePath.Length > memoryThreshold ? 
                DecompressConcatenatedStreamsLowMemoryUsage(filePath, decompressedFileName, deleteOriginal) : 
                DecompressConcatenatedStreamsHighMemoryUsage(filePath, decompressedFileName, deleteOriginal);

            return result;
        }

        /// <summary>
        /// Provides a workaround to decompressing gzip files that are concatenated
        /// I used http://www.zlib.org/rfc-gzip.html for header specification of GZip.
        /// Credit also goes to https://bamcisnetworks.wordpress.com/2017/05/22/decompressing-concatenated-gzip-files-in-c-received-from-aws-cloudwatch-logs/
        /// for describing the issue of decompressing concatenated zipped files.
        /// </summary>
        /// <param name="filePath">FileInfo of gzip concatenated file</param>
        /// <param name="decompressedFileName">Name of the decompressed file</param>
        /// <param name="deleteOriginal">Bool flag whether to remove the original file</param>
        /// <returns>The decompressed byte content of the gzip file</returns>
        public int DecompressConcatenatedStreamsHighMemoryUsage(FileInfo filePath, string decompressedFileName, bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();


            //Get the bytes of the file
            byte[] fileBytes = File.ReadAllBytes(filePath.Name);
            if (deleteOriginal)
            {
                filePath.Delete();
            }
            int traversableLength = fileBytes.Length - _startOfFilePattern.Length;
            FindMatches(startIndexes, _startOfFilePattern, fileBytes, traversableLength);

            //Start from the beginning if you did not find anything
            if (!startIndexes.Any())
            {
                startIndexes.Add(0);
            }

            List<byte[]> chunks = CreateChunksFromFile(startIndexes, fileBytes);
            return ConcatenateDecompressedChunks(chunks, decompressedFileName);
        }

        public int DecompressConcatenatedStreamsLowMemoryUsage(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();
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
                    FindMatches(startIndexes, _startOfFilePattern, concBuffer.Buffer, bufferReadCount, byteCount, traversableLength);

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
            }

            if (deleteOriginal)
            {
                filePath.Delete();
            }

            return 0;
        }

        #endregion public methods endregion

        #region private methods

        private List<byte[]> CreateChunksFromFile(List<int> startIndexes, byte[] fileBytes)
        {
            List<byte[]> chunks = new List<byte[]>();
            for (int i = 0; i < startIndexes.Count; i++)
            {
                //int start = ;
                int length = 0;

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
                    chunks.Add(fileBytes.Skip(startIndexes[i]).Take(length).ToArray());
                }
            }

            return chunks;
        }

        private void FindMatches(List<int> startIndexes, byte[] startOfFilePattern,
            byte[] buffer, int traversableLength, int bufferReadCount = 0, int byteCount = 0)
        {
            for (int i = 0; i <= traversableLength; i++)
            {
                bool match = true;
                for (int j = 0; j < startOfFilePattern.Length; j++)
                {
                    if (buffer[i + j] != startOfFilePattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match == true)
                {
                    startIndexes.Add(i + bufferReadCount * byteCount);
                    i += startOfFilePattern.Length;
                }
            }
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

        #endregion private methods endregion
    }
}
