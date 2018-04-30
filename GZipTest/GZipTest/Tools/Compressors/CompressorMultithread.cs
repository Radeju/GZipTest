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

        public CompressorMultiThread()
        { }

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
        /// Credit goes to https://bamcisnetworks.wordpress.com/2017/05/22/decompressing-concatenated-gzip-files-in-c-received-from-aws-cloudwatch-logs/
        /// for describing the issue of decompressing concatenated zipped files. I removed most of the comments, for more details
        /// please read the above blog post.
        /// </summary>
        /// <param name="filePath">FileInfo of gzip concatenated file</param>
        /// <param name="decompressedFileName">Name of the decompressed file</param>
        /// <param name="deleteOriginal">Bool flag whether to remove the original file</param>
        /// <returns>The decompressed byte content of the gzip file</returns>
        public int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName, bool deleteOriginal = false)
        {
            List<int> startIndexes = new List<int>();
            byte[] startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00 };

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
                    if (fileBytes[i + j] != startOfFilePattern[j])
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
                int start = startIndexes.ElementAt(i);
                int length = 0;

                if (i + 1 == startIndexes.Count)
                {
                    length = fileBytes.Length - start;
                }
                else
                {
                    length = startIndexes.ElementAt(i + 1) - i;
                }

                if (length > 0)
                {
                    chunks.Add(fileBytes.Skip(start).Take(length).ToArray());
                }
            }

            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (FileStream outFileStream = File.Create(decompressedFileName))
            {
                foreach (byte[] Chunk in chunks)
                {
                    using (MemoryStream mStream = new MemoryStream(Chunk))
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

                return 1;
            }
        }
    }
}
