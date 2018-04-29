using System;
using System.Collections.Generic;
using System.IO;
using GZipTest.Interfaces;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using GZipTest.Globals;

namespace GZipTest.Tools
{
    public class Compressor : ICompressor, IDisposable
    {
        private int _status;
        private FileInfo _fileToCompress;
        private string _archiveName;
        private ManualResetEvent _doneEvent;
        private bool _deleteOriginal;

        public int Status => _status;

        public Stream Fs { get; set; }

        public Compressor() { }

        public Compressor(ManualResetEvent doneEvent, FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
        {
            _doneEvent = doneEvent;
            _fileToCompress = fileToCompress;
            _archiveName = archiveName;
            _deleteOriginal = deleteOriginal;
            Fs = null;
        }

        public void ThreadPoolCallback(object threadContext)
        {
            int threadIndex = (int)threadContext;
            Console.WriteLine($"Thread {threadIndex} started...");
            _status = CompressNoDispose(_fileToCompress, _archiveName, true);
            Console.WriteLine($"Thread {threadIndex} result calculcated {Status}.");
            _doneEvent.Set();
        }

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

        public int CompressNoDispose(FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
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

        public void CompressMultiThread(FileInfo fileToCompress, string archiveName)
        {
            //string folderName = "Chunks";
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> chunks = manipulator.Split(fileToCompress.Name, Const.CHUNK_SIZE_IN_MGBS, archiveName);
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < chunks.Count; i++)
            {
                int local = i; //because of access to modified closure, not most elegant. foreach loop?
                Thread th = new Thread(() => { Compress(chunks[local], archiveName + local.ToString(), true); });
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

            manipulator.Merge(compressedChunks, archiveName);
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

        /// <summary>
        /// Provides a workaround to decompressing gzip files that are concatenated
        /// </summary>
        /// <param name="filePath">The path to the gzip file</param>
        /// <returns>The decompressed byte content of the gzip file</returns>
        public int GUnzipConcatenatedFile(string filePath, string decompressedFileName)
        {
            //Get the bytes of the file
            byte[] FileBytes = File.ReadAllBytes(filePath);

            List<int> StartIndexes = new List<int>();

            /*
            * This pattern indicates the start of a GZip file as found from looking at the files
            * The file header is 10 bytes in size
            * 0-1 Signature 0x1F, 0x8B
            * 2 Compression Method - 0x08 is for DEFLATE, 0-7 are reserved
            * 3 Flags
            * 4-7 Last Modification Time
            * 8 Compression Flags
            * 9 Operating System
            */

            byte[] StartOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00 };

            //This will limit the last byte we check to make sure it doesn't exceed the end of the file
            //If the file is 100 bytes and the file pattern is 10 bytes, the last byte we want to check is
            //90 -> i.e. we will check index 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 and index 99 is the last
            //index in the file bytes
            int TraversableLength = FileBytes.Length - StartOfFilePattern.Length;

            for (int i = 0; i <= TraversableLength; i++)
            {
                bool Match = true;

                //Test the next run of characters to see if they match
                for (int j = 0; j < StartOfFilePattern.Length; j++)
                {
                    //If the character doesn't match, break out
                    //We're making sure that i + j doesn't exceed the length as part
                    //of the loop bounds
                    if (FileBytes[i + j] != StartOfFilePattern[j])
                    {
                        Match = false;
                        break;
                    }
                }

                //If we did find a run of
                if (Match == true)
                {
                    StartIndexes.Add(i);
                    i += StartOfFilePattern.Length;
                }
            }

            //In case the pattern doesn't match, just start from the beginning of the file
            if (!StartIndexes.Any())
            {
                StartIndexes.Add(0);
            }

            List<byte[]> Chunks = new List<byte[]>();

            for (int i = 0; i < StartIndexes.Count; i++)
            {
                int Start = StartIndexes.ElementAt(i);
                int Length = 0;

                if (i + 1 == StartIndexes.Count)
                {
                    Length = FileBytes.Length - Start;
                }
                else
                {
                    Length = StartIndexes.ElementAt(i + 1) - i;
                }

                //Prevent adding an empty array, for example, if the pattern occured     
                //as the last 10 bytes of the file, there wouldn't be anything following     
                //it to represent data
                if (Length > 0)
                {
                    Chunks.Add(FileBytes.Skip(Start).Take(Length).ToArray());
                }
            }

            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (FileStream outFileStream = File.Create(decompressedFileName))
            {
                foreach (byte[] Chunk in Chunks)
                {
                    using (MemoryStream MStream = new MemoryStream(Chunk))
                    {
                        using (GZipStream GZStream = new GZipStream(MStream, CompressionMode.Decompress))
                        {
                            int bytesRead = 0;
                            while ((bytesRead = GZStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                outFileStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                return 1;
            }
        }

        public void Dispose()
        {
            Fs.Dispose();
        }
    }

    /// <summary>
    /// Created basing on https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/threading/how-to-use-a-thread-pool
    /// </summary>
    public static class ThreadPoolApproach
    {
        public static void CompressMultithread(FileInfo fileToCompress, string archiveName)
        {
            //string folderName = "Chunks";
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> chunks = manipulator.Split(fileToCompress.Name, Const.CHUNK_SIZE_IN_MGBS, archiveName);
            List<Compressor> compressors = new List<Compressor>();
            List<Stream> streams = new List<Stream>();
            ManualResetEvent[] doneEvents = new ManualResetEvent[chunks.Count];

            int i = 0;
            foreach (var chunk in chunks)
            {
                doneEvents[i] = new ManualResetEvent(false);
                Compressor c = new Compressor(doneEvents[i], chunk, archiveName + i.ToString(), true);
                compressors.Add(c);
                ThreadPool.QueueUserWorkItem(c.ThreadPoolCallback, i);
                i += 1;
            }

            WaitHandle.WaitAll(doneEvents);

            List<FileInfo> compressedChunks = new List<FileInfo>();
            foreach (var chunk in chunks)
            {
                compressedChunks.Add(new FileInfo(chunk.Name + ".gz"));
                streams.Add(File.OpenRead(chunk.Name + ".gz"));
            }

            foreach (var c in compressors)
            {
                // streams.Add(c.Fs);
            }

            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (StreamConcatenator sc = new StreamConcatenator(streams))
            {
                using (FileStream outFile = File.Create(archiveName + ".gz"))
                {
                    int bytesRead;
                    while ((bytesRead = sc.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        outFile.Write(buffer, 0, bytesRead);
                    }
                }
            }
            //manipulator.Merge(compressedChunks, archiveName);
        }
    }
}
