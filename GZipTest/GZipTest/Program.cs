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
                        //Harded coded for LowMemory usage right now as performance tests were better even for sub<50 mb files
                        IDecompressConcatenatedStreams compressor = new CompressorMultiThreadLowMemory();
                        return compressor.DecompressConcatenatedStreams(fileInfo, outputFilePath);

                    default:
                        return 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Program threw following exception {e.Message}. Shutting down.");
                return 1;
            }
        }
    }
}
