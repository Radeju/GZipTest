using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.Tools
{
    public class StreamConcatenator
    {
        private Queue<Stream> _streams;

        public StreamConcatenator(IEnumerable<Stream> streams)
        {
            _streams = new Queue<Stream>(streams);
        }
    }
}
