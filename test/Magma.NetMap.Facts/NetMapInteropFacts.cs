using System;
using System.Collections.Generic;
using System.Text;
using Magma.NetMap.Interop;
using Xunit;

namespace Magma.NetMap.Facts
{
    public class NetMapInteropFacts
    {
        [Fact]
        public unsafe void CanOpenDevice()
        {
            //NetMap.Interop.NetMapInterop.nm_open("eth0", default(nmreq), 0, null);
        }
    }
}
