using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.NetMap.TcpHost
{
    public enum TcpConnectionState
    {
        Closed,
        Listen,
        Syn_Rcvd,
        Syn_Sent,
        Established,
        Fin_Wait_1,
        Fin_Wait_2,
        Closing,
        Time_Wait,
        Close_Wait,
        Last_Ack,
    }
}
