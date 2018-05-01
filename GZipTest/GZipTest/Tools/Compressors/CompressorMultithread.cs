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
    public abstract class CompressorMultiThread : Compressor, ICompressorMultiThread
    {
        private readonly ManualResetEvent _doneEvent;
        private readonly FileInfo _fileToCompress;
        private readonly string _archiveName;
        private readonly bool _deleteOriginal;
        private int _status = 1;

        protected readonly byte[] _startOfFilePattern = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
        
        #region public methods

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
        public abstract int DecompressConcatenatedStreams(FileInfo filePath, string decompressedFileName,
            bool deleteOriginal = false);

        #endregion public methods endregion

        #region protected methods

        protected CompressorMultiThread() { }

        protected CompressorMultiThread(ManualResetEvent doneEvent, FileInfo fileToCompress, string archiveName, bool deleteOriginal = false)
        {
            _doneEvent = doneEvent;
            _fileToCompress = fileToCompress;
            _archiveName = archiveName;
            _deleteOriginal = deleteOriginal;
        }

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
