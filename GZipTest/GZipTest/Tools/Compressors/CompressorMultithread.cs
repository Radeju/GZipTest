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

        public int CompressOnMultipleThreads(FileInfo fileToCompress, string archiveName, bool deleteOriginal = true)
        {
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> chunks = manipulator.Split(fileToCompress.Name, Const.CHUNK_SIZE_IN_MGBS, archiveName);
            List<Thread> threads = new List<Thread>();
            int[] results = new int[chunks.Count];

            for (int i = 0; i < chunks.Count; i++)
            {
                int local = i; //because of access to modified closure
                Thread th = new Thread(() => { results[local] = Compress(chunks[local], archiveName + local.ToString(), deleteOriginal); });
                threads.Add(th);
                th.Start();
            }

            foreach (var th in threads)
            {
                th.Join();
            }

            List<FileInfo> compressedChunks = new List<FileInfo>();
            foreach (var chunk in chunks)
            {
                compressedChunks.Add(new FileInfo(chunk.Name + ".gz"));
            }

            //merge results back together
            manipulator.Merge(compressedChunks, archiveName);

            return results.Aggregate((x1, x2) => x1 & x2);
        }

        /// <summary>
        /// Provides a workaround to decompressing gzip files that are concatenated
        /// I used http://www.zlib.org/rfc-gzip.html for header specification of GZip.
        /// Credit goes to https://bamcisnetworks.wordpress.com/2017/05/22/decompressing-concatenated-gzip-files-in-c-received-from-aws-cloudwatch-logs/
        /// for describing the issue of decompressing concatenated zipped files. I removed most of the comments, for more details
        /// please read the above blog post. The method itself was heavily modified to be able to work against very big files.
        /// </summary>
        /// <param name="filePath">FileInfo of gzip concatenated file</param>
        /// <param name="decompressedFileName">Name of the decompressed file</param>
        /// <param name="deleteOriginal">Bool flag whether to remove the original file</param>
        /// <returns>The decompressed byte content of the gzip file</returns>
        public int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName, bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();
            byte[] startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };

            //Get the bytes of the file
            byte[] fileBytes = File.ReadAllBytes(filePath.Name);
            if (deleteOriginal)
            {
                filePath.Delete();
            }
            int traversableLength = fileBytes.Length - startOfFilePattern.Length;

            for (int i = 0; i <= traversableLength; i++)
            {
                bool match = true;
                for (int j = 0; j < startOfFilePattern.Length; j++)
                {
                    //4th bit can have all the values between 0 to 4 (5-7 reserved) depending on the input file
                    //so we cannot check it against the pattern
                    if (j != 4 && fileBytes[i + j] != startOfFilePattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match == true)
                {
                    startIndexes.Add(i);
                    i += startOfFilePattern.Length;
                }
            }

            //In case the pattern doesn't match, just start from the beginning of the file
            if (!startIndexes.Any())
            {
                startIndexes.Add(0);
            }

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

            return ConcatenateDecompressedChunks(chunks, decompressedFileName);
        }

        public class ConcBuffer
        {

        }

        public int DecompressConcatenatedStreamsLowMemory(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();
            byte[] startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //byte[] buffer = new byte[Const.CHUNK_SIZE_IN_MGBS * 1024 * 1024];
            ConcurrentBuffer concBuffer = new ConcurrentBuffer(Const.CHUNK_SIZE_IN_MGBS * 1024 * 1024);
            using (FileStream inFileStream = File.OpenRead(filePath.Name))
            {
                int bytesRead = 0;
                //wrap enough so that we do not loose filePattern
                while ((bytesRead = inFileStream.Read(concBuffer.Buffer, 0, concBuffer.MaxCount - startOfFilePattern.Length)) 
                        != 0)
                {
                    int traversableLength = concBuffer.MaxCount - startOfFilePattern.Length;
                    for (int i = 0; i <= traversableLength; i++)
                    {
                        bool match = true;
                        for (int j = 0; j < startOfFilePattern.Length; j++)
                        {
                            if (concBuffer.Buffer[i + j] != startOfFilePattern[j])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match == true)
                        {
                            startIndexes.Add(i);
                            i += startOfFilePattern.Length;
                        }
                    }
                }
            }

            if (deleteOriginal)
            {
                filePath.Delete();
            }

            return 1;
        }

        #endregion public methods endregion

        #region private methods

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
