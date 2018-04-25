using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tools
{
    internal static class Validator
    {
        /// <summary>
        /// Validates whether all arguments from the console are correct, i.e. there are exactly 3 arguments,
        /// the original_file_name exists and archive_file_name is not taken
        /// </summary>
        internal static bool ValidateArgs(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(
                    $"You need exactly 3 arguments in format \"GZipTest.exe [compress/decompress] [original_file_name] [archive_file_name]\"");
                Console.WriteLine("Please revise your arguments and try again");
#warning correct the string
                Console.WriteLine("Please remember to put the file into the same folder as your ");
                return false;
            }

            bool compress;
            switch (args[0])
            {
                case "compress":
                    compress = true;
                    break;
                case "decompress":
                    compress = false;
                    break;
                default:
                    Console.WriteLine("Your first argument must be either compress or decompress. Perhaps you made a typo. Please try again");
                    return false;
            }



            return true;
        }
    }
}
