using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest.Tools
{
    public class FileSplitter
    {
        public List<string> Split(string file, int chunkSizeInMbs, string path)
        {
            List<string> createdFiles = new List<string>();
            int chunkSize = chunkSizeInMbs * 1024 * 1024;
            byte[] buffer = new byte[16 * 1024];

            using (FileStream input = File.OpenRead(file))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    string nextFile = path + "\\" + index;
                    using (FileStream output = File.Create(nextFile))
                    {
                        createdFiles.Add(nextFile);
                        int chunkBytesRead = 0;
                        while (chunkBytesRead < chunkSize)
                        {
                            int bytesRead = input.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                            {
                                break;
                            }
                            chunkBytesRead += bytesRead;
                            output.Write(buffer, 0, bytesRead);
                        }
                    }
                    index++;
                }
            }

            return createdFiles;
        }
    }
}
