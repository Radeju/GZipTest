using System.IO;

namespace GZipTest.Interfaces
{
    public interface ICompressor
    {
        int Compress(FileInfo fileToCompress, string archiveName);
        int Decompress(FileInfo fileToDecompress, string decompressedFileName);
    }
}
