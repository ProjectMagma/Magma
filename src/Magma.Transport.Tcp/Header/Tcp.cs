
using System.Runtime.InteropServices;

namespace Magma.Network.Header
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tcp
    {
        /// <summary>
        /// Identifies the sending port.
        /// </summary>
        public ushort SourcePort;

        /// <summary>
        /// Identifies the receiving port.
        /// </summary>
        public ushort DestinationPort;

        /// <summary>
        /// Has a dual role: If the SYN flag is set(1), then this is the initial sequence number. 
        /// The sequence number of the actual first data byte and the acknowledged number in the corresponding ACK are then this sequence number plus 1.
        /// If the SYN flag is clear (0), then this is the accumulated sequence number of the first data byte of this segment for the current session.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// If the ACK flag is set then the value of this field is the next sequence number that the sender of the ACK is expecting.
        /// This acknowledges receipt of all prior bytes (if any). The first ACK sent by each end acknowledges the other end's initial sequence number itself, but no data.
        /// </summary>
        public uint AcknowledgmentNumber;

        /// <summary>
        /// Specifies the size of the TCP header in 32-bit words.
        /// The minimum size header is 5 words and the maximum is 15 words thus giving the minimum size of 20 bytes and maximum of 60 bytes, 
        /// allowing for up to 40 bytes of options in the header. 
        /// This field gets its name from the fact that it is also the offset from the start of the TCP segment to the actual data.
        /// </summary>
        public byte DataOffset;

        private byte _reservedAndFlags0;
        /// <summary>
        /// For future use and should be set to zero. (3 bits)
        /// </summary>
        public byte Reserved => (byte)(_reservedAndFlags0 >> 1);

        /// <summary>
        /// ECN-nonce - concealment protection(experimental: see RFC 3540)
        /// </summary>
        public bool NS => (_reservedAndFlags0 & 0b1) == 0 ? false : true;

        private ushort _flags1;

        /// <summary>
        /// Congestion Window Reduced flag is set by the sending host to indicate that it received a TCP segment 
        /// with the ECE flag set and had responded in congestion control mechanism (added to header by RFC 3168).
        /// </summary>
        public bool CWR => (_flags1 & 0b_1000_0000) == 0 ? false : true;

        /// <summary>
        /// ECN-Echo has a dual role, depending on the value of the SYN flag.
        /// It indicates: If the SYN flag is set (1), that the TCP peer is ECN capable.
        /// If the SYN flag is clear (0), that a packet with Congestion Experienced flag set(ECN= 11) in the IP header was received 
        /// during normal transmission(added to header by RFC 3168). 
        /// This serves as an indication of network congestion (or impending congestion) to the TCP sender.
        /// </summary>
        public bool ECE => (_flags1 & 0b_0100_0000) == 0 ? false : true;

        /// <summary>
        /// Indicates that the Urgent pointer field is significant
        /// </summary>
        public bool URG => (_flags1 & 0b_0010_0000) == 0 ? false : true;
        /// <summary>
        /// Indicates that the Acknowledgment field is significant.
        /// All packets after the initial SYN packet sent by the client should have this flag set.
        /// </summary>
        public bool ACK => (_flags1 & 0b_0001_0000) == 0 ? false : true;

        /// <summary>
        /// Push function. Asks to push the buffered data to the receiving application.
        /// </summary>
        public bool PSH => (_flags1 & 0b_0000_1000) == 0 ? false : true;

        /// <summary>
        /// Reset the connection
        /// </summary>
        public bool RST => (_flags1 & 0b_0000_0100) == 0 ? false : true;

        /// <summary>
        /// Synchronize sequence numbers.
        /// Only the first packet sent from each end should have this flag set.
        /// Some other flags and fields change meaning based on this flag, and some are only valid when it is set, and others when it is clear.
        /// </summary>
        public bool SYN => (_flags1 & 0b_0000_0010) == 0 ? false : true;

        /// <summary>
        /// Last packet from sender.
        /// </summary>
        public bool FIN => (_flags1 & 0b_0000_0001) == 0 ? false : true;

        /// <summary>
        /// The size of the receive window, which specifies the number of bytes that 
        /// the sender of this segment is currently willing to receive.
        /// </summary>
        public ushort WindowSize;

        /// <summary>
        /// The 16-bit checksum field is used for error-checking of the header, the Payload and a Pseudo-Header.
        /// The Pseudo-Header consists of the Source IP Address, the Destination IP Address, 
        /// the protocol number for the TCP-Protocol (0x0006) and the length of the TCP-Headers including Payload (in Bytes).
        /// </summary>
        public ushort Checksum;

        /// <summary>
        /// If the URG flag is set, then this 16-bit field is an offset from the sequence number indicating the last urgent data byte.
        /// </summary>
        /// <returns></returns>
        public ushort UrgentPointer;

        // Options: (Variable 0–320 bits, divisible by 32)
        // Padding: The TCP header padding is used to ensure that the TCP header ends, and data begins, on a 32 bit boundary.
        //          The padding is composed of zeros.
    }
}
