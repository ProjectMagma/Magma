using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Internet.Icmp
{
    public enum ControlMessage : byte
    {
        EchoReply = 0,
        Unassigned1 = 1,
        Unassigned2 = 2,
        DestinationUnreachable = 3,
        SourceQuench = 4,
        RedirectMessage = 5,
        Deprecated6 = 6,
        Unassigned7 = 7,
        EchoRequest = 8,
        RouterAdvertisement = 9,
        RouterSolicitation = 10,
        TimeExceeded = 11,
        ParameterProblem = 12,
        Timestamp = 13,
        TimestampReply = 14,
        InformationRequest = 15,
        InformationReply = 16,
        AddressMaskRequest = 17,
        AddressMaskReply = 18,

        Traceroute = 30
    }
}
