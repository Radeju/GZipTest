using System;
using System.IO;
using GZipTest.Enums;

namespace GZipTest.Tools
{
    internal class Validator
    {

        #region public methods
        /// <summary>
        /// Validates whether all arguments from the console are correct, i.e. there are exactly 3 arguments,
        /// the original_file_name exists and archive_file_name is not taken
        /// </summary>
        internal bool ValidateArgs(string[] args, out string inputFilePath, out CompressOperations compress)
        {
            compress = CompressOperations.Decompress;
            inputFilePath = null;

            if (!ValidateLength(args))
            {
                return false;
            }

            if (!ValidateOperation(args, out compress))
            {
                return false;
            }

            if (!ValidateInputPath(args, out inputFilePath))
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

        private bool ValidateInputPath(string[] args, out string inputFilePath)
        {
            
            string dirPath = Environment.CurrentDirectory;
            string filePath = dirPath + "\\" + args[1];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($@"Unfortunately the file that you selected {filePath} does not exist. Please try once more. You only need to write
                    the name of the file within the directory of the program");
                inputFilePath = null;
                return false;
            }
            inputFilePath = filePath;
            return true;
        }

        #endregion private methods
    }
}
