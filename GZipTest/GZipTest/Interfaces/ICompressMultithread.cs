using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Interfaces
{
    public interface ICompressorMultithread : ICompressor
    {
        int CompressMultiThread(FileInfo fileTocompress, string archiveName);
        int DecompressConcatenatedStreams(FileInfo file, string decompressedFileName);
    }
}
