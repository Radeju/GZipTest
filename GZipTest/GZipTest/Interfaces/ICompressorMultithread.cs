using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Interfaces
{
    public interface ICompressorMultiThread : ICompressor
    {
        void ThreadPoolCallback(object threadContext);
        int DecompressConcatenatedStreams(FileInfo file, string decompressedFileName, bool deleteOriginal = false);
        int Status();
    }
}
