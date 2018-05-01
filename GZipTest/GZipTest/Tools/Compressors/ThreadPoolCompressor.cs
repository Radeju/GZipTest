using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.Globals;
using GZipTest.Interfaces;

namespace GZipTest.Tools.Compressors
{
    /// <summary>
    /// Created basing on https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/threading/how-to-use-a-thread-pool
    /// </summary>
    public class ThreadPoolCompressor : IThreadPoolCompressor
    {
        public int ThreadPoolCompress(FileInfo fileToCompress, string archiveName)
        {
            FileManipulator manipulator = new FileManipulator();
            List<FileInfo> chunks = manipulator.Split(fileToCompress.Name, Const.CHUNK_SIZE_IN_MGBS, archiveName);
            ManualResetEvent[] doneEvents = new ManualResetEvent[chunks.Count];
            List<ICompressorMultiThread> compressors = new List<ICompressorMultiThread>();

            int i = 0;
            //compress each chunk separately using ThreadPool
            foreach (var chunk in chunks)
            {
                doneEvents[i] = new ManualResetEvent(false);
#warning think how to improve this
                ICompressorMultiThread c = new CompressorMultiThreadLowMemory(doneEvents[i], chunk, archiveName + i.ToString(), true);
                compressors.Add(c);
                ThreadPool.QueueUserWorkItem(c.ThreadPoolCallback, i);
                i += 1;
            }

            WaitHandle.WaitAll(doneEvents);
            
            List<FileInfo> compressedChunks = new List<FileInfo>();
            foreach (var chunk in chunks)
            {
                compressedChunks.Add(new FileInfo(chunk.Name + ".gz"));
            }

            //merge results back together
            manipulator.Merge(compressedChunks, archiveName);

            //bit AND on results of each of the compressors
            return compressors.Select(c => c.Status()).Aggregate((x1, x2) => x1 & x2);
        }
    }
}