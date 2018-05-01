using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Interfaces
{
    public interface ICompressorMultithread : ICompressor
    {
        void ThreadPoolCallback(object threadContext);
        int CompressOnMultipleThreads(FileInfo fileTocompress, string archiveName, bool deleteOriginal = true);
        int DecompressConcatenatedStreams(FileInfo file, string decompressedFileName, bool deleteOriginal = false);
    }
}
