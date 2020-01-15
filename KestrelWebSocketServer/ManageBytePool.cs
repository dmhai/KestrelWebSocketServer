using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Buffers;

namespace KestrelWebSocketServer
{
    public class ManageBytePool<T>
    {
        public ArrayPool<T> Pool { get; internal set; }

        public ManageBytePool(int arrayLength,int arrayCount)
        {
            Pool = ArrayPool<T>.Create(arrayLength,arrayCount);
        }
    }
}
