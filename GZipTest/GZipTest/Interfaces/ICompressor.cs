using System.IO;

namespace GZipTest.Interfaces
{
    public interface ICompressor
    {
        int Compress(FileInfo fileToCompress, string archiveName, bool deleteOriginal = false);
        int Decompress(FileInfo fileToDecompress, string decompressedFileName, bool deleteOriginal = false);
    }
}
