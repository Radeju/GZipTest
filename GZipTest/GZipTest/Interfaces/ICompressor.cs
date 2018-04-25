using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Interfaces
{
    public interface ICompressor
    {
        int Compress();
        int Decompress();
    }
}
