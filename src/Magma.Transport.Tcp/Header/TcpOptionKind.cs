using System;
using System.Collections.Generic;
using System.Text;

namespace Magma.Transport.Tcp.Header
{
    public enum TcpOptionKind : byte
    {
        EndOfOptions = 0,
        NoOp = 1,
        MaximumSegmentSize = 2,
        WindowScale = 3,
        SackPermitted = 4,
        Sack = 5,
        Echo = 6,
        EchoReply = 7,
        Timestamps = 8,
        PartialOrderConnectionPermitted = 9,
        PartialOrderServiceProfile = 10,
        CC = 11,
        CCNew = 12,
        CCEcho = 13,
        TcpAlternateChecksumRequested = 14,
        TcpAlternateChecksumData = 15,
        Skeeter = 16,
        Bubba = 17,
        TrailerChecksumOption = 18,
        MD5ChecksumOption = 19,
        ScpsCapabilities = 20,
        SelectiveNegativeAcknowledgements = 21,
        RecordBoundaries = 22,
        CorruptionExperinced = 23,
        Snap = 24,
        Unassigned = 25,
        TcpCompressionFilter = 26,
        QuickStartResponse = 27,
        UserTimeoutOption = 28,
        TcpAuthenticationOption = 29,
        MultiPathTcp = 30,
        TcpFastOpenCookie = 34,
    }
}
