using System;

namespace Magma.Transport.Tcp.Header
{
    [Flags]
    public enum TcpFlags : int
    {
        None = 0,

        NS  = 0b1_0000_0000,
        CWR = 0b0_1000_0000,
        ECE = 0b0_0100_0000,
        URG = 0b0_0010_0000,
        ACK = 0b0_0001_0000,
        PSH = 0b0_0000_1000,
        RST = 0b0_0000_0100,
        SYN = 0b0_0000_0010,
        FIN = 0b0_0000_0001
    }
}
