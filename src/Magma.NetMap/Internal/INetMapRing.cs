using System;
using System.Collections.Generic;
using System.Text;
using Magma.NetMap.Interop;

namespace Magma.NetMap.Internal
{
    internal interface INetMapRing : IDisposable
    {
        NetMapBufferPool BufferPool { set; }
        void Start();
        NetMapRing NetMapRing { get; }
        void Return(int buffer_index);
    }
}
