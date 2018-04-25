using System;
using System.Collections.Generic;
using System.IO;
using GZipTest.Enums;
using System.Linq;
using System.Text;

namespace GZipTest.Tools
{
    internal class Validator
    {

        #region public methods
        /// <summary>
        /// Validates whether all arguments from the console are correct, i.e. there are exactly 3 arguments,
        /// the original_file_name exists and archive_file_name is not taken
        /// </summary>
        internal bool ValidateArgs(string[] args, out CompressOperations compress)
        {
            compress = CompressOperations.Decompress;

            if (!ValidateLength(args))
            {
                return false;
            }

            if (!ValidateOperation(args, out compress))
            {
                return false;
            }

            if (!ValidateInputPath(args))
            {
                return false;
            }
            
            return true;
        }

        #endregion public methods

        #region private methods

        private bool ValidateLength(string[] args)
        {
            if (args.Length == 3)
            {
                return true;
            }                
            
            Console.WriteLine(
                $"You need exactly 3 arguments in format \"GZipTest.exe [compress/decompress] [original_file_name] [archive_file_name]\"");
            Console.WriteLine("Please remember to put the file you compress/decompress into the same folder as your program.");
            Console.WriteLine("Please revise your arguments and try again.");
            return false;
        }

        private bool ValidateOperation(string[] args, out CompressOperations compress)
        {
            compress = CompressOperations.Decompress;
            switch (args[0])
            {
                case "compress":
                    compress = CompressOperations.Compress;
                    break;
                case "decompress":
                    break;
                default:
                    Console.WriteLine("Your first argument must be either compress or decompress. Perhaps you made a typo. Please try again");
                    return false;
            }
            return true;
        }

        private bool ValidateInputPath(string[] args)
        {
            string path = Environment.CurrentDirectory;
            FileInfo inFileInfo = new FileInfo(path + args[1]);
            if (inFileInfo == null)
            {
                Console.WriteLine(@"Unfortunately the file that you selected does not exist. Please try once more. You only need to write
                    the name of the file within the directory of the program");
                return false;
            }
            return true;
        }

        #endregion private methods
    }
}
