using System.IO;

namespace GZipTest.Interfaces
{
    public interface IThreadPoolCompressor
    {
        int ThreadPoolCompress(FileInfo fileToCompress, string archiveName);
    }
}
