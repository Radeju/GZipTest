using System;
using System.IO;
using System.Threading;
using GZipTest.Tools;
using GZipTest.Enums;
using GZipTest.Interfaces;

namespace GZipTest
{
    public class Program
    {   
        public static int Main(string[] args)
        {
            try {
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

                ICompressor compressor = new Compressor();
                FileInfo fileInfo = new FileInfo(inputFilePath);
                switch (compressOperation)
                {
                    case CompressOperations.Compress:
                        /*
                        int myVal;
                        Thread thread = new Thread( () =>
                        {
                            myVal = compressor.Compress(fileInfo, outputFilePath);
                            Console.WriteLine("Finished");
                        });
                        thread.Start();
                        return 1;
                        */
                        return compressor.Compress(fileInfo, outputFilePath);

                    case CompressOperations.Decompress:
                        return compressor.Decompress(fileInfo, outputFilePath);

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
