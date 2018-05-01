using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZipTest.Tools
{
    public class ConcurrentBuffer
    {
        private readonly LinkedList<byte> _bufferList;
        private byte[] _buffer;
        private bool _bufferChanged;
        public int MaxCount { get; }
        
        public byte[] Buffer
        {
            get
            {
                lock (_bufferList)
                {
                    if (_bufferChanged || _buffer == null)  //logic to reduce amount of .ToArray() calls
                    {
                        _buffer = _bufferList.ToArray();
                        _bufferChanged = false;
                    }

                    return _buffer;
                }
            }
            set
            {
                lock (_bufferList)
                {
                    foreach (byte b in value)
                    {
                        PutNoLock(b);
                    }
                    _bufferChanged = true;
                }
            }
        }

        public ConcurrentBuffer(int maxCount)
        {
            MaxCount = maxCount;
            _bufferList = new LinkedList<byte>();
            _bufferChanged = true;
        }

        public void Put(byte b)
        {
            lock (_bufferList)
            {
                PutNoLock(b);
            }

            _bufferChanged = true;
        }

        private void PutNoLock(byte b)
        {
            _bufferList.AddFirst(b);
            if (_bufferList.Count > MaxCount)
            {
                _bufferList.RemoveLast();
            }
        }
    }
}
