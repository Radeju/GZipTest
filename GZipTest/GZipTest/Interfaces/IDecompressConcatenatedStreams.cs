using System.IO;

namespace GZipTest.Interfaces
{
    public interface IDecompressConcatenatedStreams
    {
        int DecompressConcatenatedStreams(FileInfo file, string decompressedFileName, bool deleteOriginal = false);
    }
}
