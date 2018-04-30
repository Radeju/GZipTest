using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Interfaces
{
    public interface ICompressorMultithread : ICompressor
    {
        void CompressMultiThread(FileInfo fileTocompress, string archiveName);
        int DecompressConcatenatedStreams(string file, string decompressedFileName);
    }
}
