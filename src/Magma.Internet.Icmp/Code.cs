using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Internet.Icmp
{
    public enum Code : short
    {
        EchoReply =  0x00 << 8 | ControlMessage.EchoReply,

        DestinationNetworkUnreachable = 0x00 << 8 | ControlMessage.DestinationUnreachable,
        DestinationHostUnreachable = 0x01 << 8 | ControlMessage.DestinationUnreachable,
        DestinationProtocolUnreachable = 0x02 << 8 | ControlMessage.DestinationUnreachable,
        DestinationPortUnreachable = 0x03 << 8 | ControlMessage.DestinationUnreachable,
        FragmentationRequired = 0x04 << 8 | ControlMessage.DestinationUnreachable,
        SourcRouteFailed = 0x05 << 8 | ControlMessage.DestinationUnreachable,
        DestinationNetworkUnknown = 0x06 << 8 | ControlMessage.DestinationUnreachable,
        DestinationHostUnknown = 0x07 << 8 | ControlMessage.DestinationUnreachable,
        SourceHostIsolated = 0x08 << 8 | ControlMessage.DestinationUnreachable,
        NetworkAdministrativelyProhibited = 0x09 << 8 | ControlMessage.DestinationUnreachable,
        HostAdministrativelyProhibited = 0x0A << 8 | ControlMessage.DestinationUnreachable,
        NetworkUnreachableForToS = 0x0B << 8 | ControlMessage.DestinationUnreachable,
        HostUnreachableForToS = 0x0C << 8 | ControlMessage.DestinationUnreachable,
        CommunicationAdministrativelyProhibited = 0x0D << 8 | ControlMessage.DestinationUnreachable,
        HostPrecedenceViolation = 0x0E << 8 | ControlMessage.DestinationUnreachable,
        PrecedenceCutoffInEffect = 0x0F << 8 | ControlMessage.DestinationUnreachable,

        EchoRequest = 0x00 << 8 | ControlMessage.EchoRequest

        // TODO: Other codes
    }
}
