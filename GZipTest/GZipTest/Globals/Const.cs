using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Globals
{
    public static class Const
    {
        public const int BUFFER_SIZE = 64 * 1024;
        public const int CHUNK_SIZE_IN_MGBS = 16;
        public const long MEMORY_THRESHOLD = 100 * 1024 * 1024;
    }
}
