using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace KestrelWebSocketServer
{
    public class MemoryBytePool : IDisposable
    {
        private MemoryPool<byte> Pool { get => MemoryPool<byte>.Shared; }

        public IMemoryOwner<byte> GetBuffer(int length)
        {
            return Pool.Rent(length);
        }

        public void Dispose()
        {
            Pool.Dispose();
        }

    }
}
