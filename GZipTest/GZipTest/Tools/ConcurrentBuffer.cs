using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tools
{
    /// <summary>
    /// Very trivial concurrent buffer implementation.
    /// </summary>
    public class ConcurrentBuffer
    {
        private byte[] _buffer;
        private int _index;

        public int MaxCount { get; }

        public int Index
        {
            get { return _index; }
            set
            {
                if (value >= MaxCount)
                {
                    _index = value % MaxCount;
                }

            }
        }

        public byte[] Buffer
        {
            get
            {
                lock (_buffer)
                {
                    return _buffer;
                }
            }
            set
            {
                lock (_buffer)
                {
                    int srcLen = value.Length;
                    int start = 0;
                    int countToEnd = Math.Min(srcLen, MaxCount - _index);
                    do
                    {
                        Array.Copy(value, start, _buffer, _index, countToEnd);
                        start = start + countToEnd;
                        srcLen -= countToEnd;
                        _index += countToEnd;
                    } while (srcLen > 0);
                }
            }
        }

        public ConcurrentBuffer(int maxCount)
        {
            MaxCount = maxCount;
            _index = 0;
            _buffer = new byte[MaxCount];
        }

        public void MoveLastBytesToBeginning(int byteCount)
        {
            if (byteCount >= MaxCount)
            {
                byteCount = byteCount % MaxCount;
            }

            for (int i = 0; i < byteCount; i++)
            {
                _buffer[i] = Buffer[MaxCount - byteCount + i];
            }
        }
    }
}
