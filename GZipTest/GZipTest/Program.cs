using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Compression;
using System.IO;
using GZipTest.Tools;
using GZipTest.Enums;
using GZipTest.Interfaces;

namespace GZipTest
{
    class Program
    {   
        static int Main(string[] args)
        {
            try {
                Validator val = new Validator();
                CompressOperations compressOperation;
                if (!val.ValidateArgs(args, out compressOperation))
                {
                    Console.WriteLine("Validator has finished validation unsuccessfully. Please review output above for more details");
                    return 1;
                }

                PathCorrector corrector = new PathCorrector();
                FileInfo outputFileInfo = corrector.CorrectOutput(args[2]);

                ICompressor compressor = new Compressor();
                if (compressOperation == CompressOperations.Compress)
                {
                    return compressor.Compress();
                }
                if (compressOperation == CompressOperations.Decompress)
                {
                    return compressor.Decompress();
                }

                //Program will get here only if we add another CompressOperations
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Program threw following exception {e.Message}. Shutting down");
                return 1;
            }
        }
    }
}
