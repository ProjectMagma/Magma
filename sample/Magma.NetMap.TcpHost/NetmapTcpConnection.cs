using System;
using System.Collections.Generic;
using System.Text;
using Magma.Network.Header;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Magma.NetMap.TcpHost
{
    public class NetmapTcpConnection
    {
        private TcpReceiver _tcpReceiver;
        private TransportConnection _connection;
        private TcpConnectionState _state = TcpConnectionState.Listen;
        private uint _receiveSequenceNumber;
        private uint _sendSequenceNumber;
        private ushort _remotePort;
        private ushort _localPort;
                
        public NetmapTcpConnection(TcpReceiver tcpReceiver)
        {
            _tcpReceiver = tcpReceiver;
            _connection = new TransportConnection();
        }

        public TransportConnection Connection => _connection;

        public void ProcessPacket(Tcp header, ReadOnlySpan<byte> data)
        {
            switch(_state)
            {
                case TcpConnectionState.Listen:
                    // We know we checked for syn in the upper layer so we can ignore that for now
                    _receiveSequenceNumber = header.SequenceNumber;
                    _sendSequenceNumber = _tcpReceiver.RandomSeqeunceNumber();
                    var responseTcp = new Tcp
                    {
                        ACK = true,
                        SourcePort = _localPort,
                        DestinationPort = _remotePort,
                        AcknowledgmentNumber = _receiveSequenceNumber + 1,
                        SequenceNumber = _sendSequenceNumber,
                        SYN = true
                    };

                    _state = TcpConnectionState.Syn_Rcvd;
                    break;
            }
        }
    }
}
