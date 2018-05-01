using System;
using System.IO;
using GZipTest.Tools;
using GZipTest.Enums;
using GZipTest.Globals;
using GZipTest.Interfaces;
using GZipTest.Tools.Compressors;

namespace GZipTest
{
    public class Program
    {   
        public static int Main(string[] args)
        {
            try
            {
                args = new[] {"compress", "standard.xml", "standard-comp.gz"};
                //args = new[] { "decompress", "standard-comp.gz", "standard-restored" };

                Validator val = new Validator();
                CompressOperations compressOperation;
                string inputFilePath;
                if (!val.ValidateArgs(args, out inputFilePath, out compressOperation))
                {
                    Console.WriteLine("Validator has finished validation unsuccessfully. Please review output above for more details");
                    return 1;
                }

                PathCorrector corrector = new PathCorrector();
                string outputFilePath = corrector.CorrectOutput(args[2]);
                FileInfo fileInfo = new FileInfo(inputFilePath);

                switch (compressOperation)
                {
                    case CompressOperations.Compress:
                        IThreadPoolCompressor ctp = new ThreadPoolCompressor();
                        return ctp.ThreadPoolCompress(fileInfo, outputFilePath);

                    case CompressOperations.Decompress:
                        long size = fileInfo.Length;
                        ICompressorMultiThread compressor = size > Const.MEMORY_THRESHOLD ? 
                            (ICompressorMultiThread) new CompressorMultiThreadLowMemory() :
                            (ICompressorMultiThread) new CompressorMultiThreadHighMemory();
                        return compressor.DecompressConcatenatedStreams(fileInfo, outputFilePath);

                    default:
                        return 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Program threw following exception {e.Message}. Shutting down");
                return 1;
            }
        }
    }
}
