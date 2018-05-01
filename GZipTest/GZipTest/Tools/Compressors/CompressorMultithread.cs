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
    public class CompressorMultiThread : Compressor, ICompressorMultiThread
    {
        private readonly ManualResetEvent _doneEvent;
        private readonly FileInfo _fileToCompress;
        private readonly string _archiveName;
        private readonly bool _deleteOriginal;
        private int _status = 1;

        protected readonly byte[] _startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };


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
            _status = Compress(_fileToCompress, _archiveName, _deleteOriginal);
            _doneEvent.Set();
        }

        public int Status()
        {
            return _status;
        }

        public virtual int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false)
        {
            return 1;

            /*
            List<int> startIndexes = new List<int>();
            byte[] startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
            const long memoryThreshold = 100 * 1024 * 1024;

            int result = filePath.Length > memoryThreshold ? 
                DecompressConcatenatedStreamsLowMemoryUsage(filePath, decompressedFileName, deleteOriginal) : 
                DecompressConcatenatedStreamsHighMemoryUsage(filePath, decompressedFileName, deleteOriginal);

            return result;*/
        }

        

        

        #endregion public methods endregion

        #region protected methods



        

        protected void FindMatches(List<long> startIndexes, byte[] startOfFilePattern,
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

        

        

        #endregion protected methods endregion
    }
}
