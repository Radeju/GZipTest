using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GZipTest.Globals;
using GZipTest.Interfaces;

namespace GZipTest.Tools.Compressors
{
    public class CompressorMultithread : Compressor, ICompressorMultithread
    {
        private readonly ManualResetEvent _doneEvent;
        private readonly FileInfo _fileToCompress;
        private readonly string _archiveName;
        private bool _deleteOriginal;

        private int _status;
        public int Status => _status;

        public CompressorMultithread()
        { }

        public CompressorMultithread(ManualResetEvent doneEvent, FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
        {
            _doneEvent = doneEvent;
            _fileToCompress = fileToCompress;
            _archiveName = archiveName;
            _deleteOriginal = deleteOriginal;
        }

        public void ThreadPoolCallback(object threadContext)
        {
            int threadIndex = (int)threadContext;
            //Console.WriteLine($"Thread {threadIndex} started...");
            _status = Compress(_fileToCompress, _archiveName, true);
            //Console.WriteLine($"Thread {threadIndex} result calculcated {Status}.");
            _doneEvent.Set();
        }

        public void CompressMultiThread(FileInfo fileToCompress, string archiveName)
        {
            //string folderName = "Chunks";
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> chunks = manipulator.Split(fileToCompress.Name, Const.CHUNK_SIZE_IN_MGBS, archiveName);
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < chunks.Count; i++)
            {
                int local = i; //because of access to modified closure
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
    }
}
