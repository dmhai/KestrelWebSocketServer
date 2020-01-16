using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;

namespace KestrelWebSocketServer
{
    internal class BufferArrayPool : IDisposable
    {
        public byte[] Buffer { get; set; }

        public void Dispose() => ArrayPool<byte>.Shared.Return(Buffer);

        public BufferArrayPool(int _length)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(_length);
        }

    }
}
