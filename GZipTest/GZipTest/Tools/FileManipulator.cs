using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.Globals;

namespace GZipTest.Tools
{
    public class FileManipulator
    {
        public List<FileInfo> Split(string file, int chunkSizeInMbs, string chunkName, string folderName = null)
        {
            List<FileInfo> createdFiles = new List<FileInfo>();
            int chunkSize = chunkSizeInMbs * 1024 * 1024;
            byte[] buffer = new byte[Const.BUFFER_SIZE];

            using (FileStream input = File.OpenRead(file))
            {
                int index = 0;
                if (folderName != null)
                {
                    Directory.CreateDirectory(folderName);
                }

                while (input.Position < input.Length)
                {
                    string nextFile = chunkName + index;
                    string nextPath = folderName == null ? nextFile : folderName + "\\" + nextFile;
                    FileInfo nextFileInfo = new FileInfo(nextFile);

                    using (FileStream output = File.Create(nextPath))
                    {
                        createdFiles.Add(nextFileInfo);
                        int bytesRead;
                        int remaining = chunkSize;
                        while (remaining > 0 && 
                               (bytesRead = input.Read(buffer, 0, Math.Min(remaining, Const.BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }
                    }
                    index++;
                }
            }

            return createdFiles;
        }

        public void Merge(List<FileInfo> filesToMerge, string outFileName)
        {
            if (outFileName == null || filesToMerge.Count == 0)
            {
                return;
            }
            
            string commonExtension = filesToMerge.Select(fi => fi.Extension)
                                           .Aggregate((x1, x2) => x1 == x2 ? x1 : string.Empty);

            byte[] buffer = new byte[Const.BUFFER_SIZE];
            using (FileStream outFileStream = File.Create(outFileName + commonExtension))
            {
                foreach (var fi in filesToMerge)
                {
                    using (FileStream nextFile = File.OpenRead(fi.Name))
                    {
                        int bytesRead;
                        while ( (bytesRead = nextFile.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outFileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    fi.Delete();
                }
            }
        }
    }
}
