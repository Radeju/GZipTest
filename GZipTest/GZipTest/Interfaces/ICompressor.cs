using System.IO;

namespace GZipTest.Interfaces
{
    public interface ICompressor
    {
        int Compress(FileInfo fileToCompress, string archiveName, bool deleteOriginal = false);
        void CompressMultiThread(FileInfo fileTocompress, string archiveName);
        int Decompress(FileInfo fileToDecompress, string decompressedFileName, bool deleteOriginal = false);
        int GUnzipConcatenatedFile(string file, string decompressedFileName);
    }
}
