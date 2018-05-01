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
        int DecompressConcatenatedStreamsHighMemoryUsage(FileInfo file, string decompressedFileName, bool deleteOriginal = false);
    }
}
