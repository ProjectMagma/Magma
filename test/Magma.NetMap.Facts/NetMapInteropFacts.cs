using System;
using System.Collections.Generic;
using System.Text;
using Magma.NetMap.Interop;
using Xunit;

namespace Magma.NetMap.Facts
{
    public class NetMapInteropFacts
    {
        [Fact(Skip = "Can only run on a machine with netmap")]
        public unsafe void CanOpenDevice()
        {
            //NetMap.Interop.NetMapInterop.nm_open("eth0", default(nmreq), 0, null);
        }
    }
}
