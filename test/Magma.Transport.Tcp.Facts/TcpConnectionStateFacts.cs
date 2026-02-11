using System;
using Xunit;

namespace Magma.Transport.Tcp.Facts
{
    public class TcpConnectionStateFacts
    {
        [Fact]
        public void TcpConnectionStateEnumValues()
        {
            Assert.Equal(0, (int)TcpConnectionState.Closed);
            Assert.Equal(1, (int)TcpConnectionState.Listen);
            Assert.Equal(2, (int)TcpConnectionState.Syn_Rcvd);
            Assert.Equal(3, (int)TcpConnectionState.Syn_Sent);
            Assert.Equal(4, (int)TcpConnectionState.Established);
            Assert.Equal(5, (int)TcpConnectionState.Fin_Wait_1);
            Assert.Equal(6, (int)TcpConnectionState.Fin_Wait_2);
            Assert.Equal(7, (int)TcpConnectionState.Closing);
            Assert.Equal(8, (int)TcpConnectionState.Time_Wait);
            Assert.Equal(9, (int)TcpConnectionState.Close_Wait);
            Assert.Equal(10, (int)TcpConnectionState.Last_Ack);
        }

        [Fact]
        public void StateTransitionClosedToListen()
        {
            var state = TcpConnectionState.Closed;
            state = TcpConnectionState.Listen;
            Assert.Equal(TcpConnectionState.Listen, state);
        }

        [Fact]
        public void StateTransitionListenToSynRcvd()
        {
            var state = TcpConnectionState.Listen;
            state = TcpConnectionState.Syn_Rcvd;
            Assert.Equal(TcpConnectionState.Syn_Rcvd, state);
        }

        [Fact]
        public void StateTransitionSynRcvdToEstablished()
        {
            var state = TcpConnectionState.Syn_Rcvd;
            state = TcpConnectionState.Established;
            Assert.Equal(TcpConnectionState.Established, state);
        }

        [Fact]
        public void StateTransitionEstablishedToFinWait1()
        {
            var state = TcpConnectionState.Established;
            state = TcpConnectionState.Fin_Wait_1;
            Assert.Equal(TcpConnectionState.Fin_Wait_1, state);
        }

        [Fact]
        public void StateTransitionFinWait1ToFinWait2()
        {
            var state = TcpConnectionState.Fin_Wait_1;
            state = TcpConnectionState.Fin_Wait_2;
            Assert.Equal(TcpConnectionState.Fin_Wait_2, state);
        }

        [Fact]
        public void StateTransitionFinWait2ToTimeWait()
        {
            var state = TcpConnectionState.Fin_Wait_2;
            state = TcpConnectionState.Time_Wait;
            Assert.Equal(TcpConnectionState.Time_Wait, state);
        }

        [Fact]
        public void StateTransitionTimeWaitToClosed()
        {
            var state = TcpConnectionState.Time_Wait;
            state = TcpConnectionState.Closed;
            Assert.Equal(TcpConnectionState.Closed, state);
        }

        [Fact]
        public void StateTransitionEstablishedToCloseWait()
        {
            var state = TcpConnectionState.Established;
            state = TcpConnectionState.Close_Wait;
            Assert.Equal(TcpConnectionState.Close_Wait, state);
        }

        [Fact]
        public void StateTransitionCloseWaitToLastAck()
        {
            var state = TcpConnectionState.Close_Wait;
            state = TcpConnectionState.Last_Ack;
            Assert.Equal(TcpConnectionState.Last_Ack, state);
        }

        [Fact]
        public void StateTransitionLastAckToClosed()
        {
            var state = TcpConnectionState.Last_Ack;
            state = TcpConnectionState.Closed;
            Assert.Equal(TcpConnectionState.Closed, state);
        }

        [Fact]
        public void StateTransitionSynSentToEstablished()
        {
            var state = TcpConnectionState.Syn_Sent;
            state = TcpConnectionState.Established;
            Assert.Equal(TcpConnectionState.Established, state);
        }

        [Fact]
        public void StateTransitionFinWait1ToClosing()
        {
            var state = TcpConnectionState.Fin_Wait_1;
            state = TcpConnectionState.Closing;
            Assert.Equal(TcpConnectionState.Closing, state);
        }

        [Fact]
        public void StateTransitionClosingToTimeWait()
        {
            var state = TcpConnectionState.Closing;
            state = TcpConnectionState.Time_Wait;
            Assert.Equal(TcpConnectionState.Time_Wait, state);
        }

        [Fact]
        public void AllStateNamesAreDefined()
        {
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Closed));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Listen));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Syn_Rcvd));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Syn_Sent));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Established));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Fin_Wait_1));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Fin_Wait_2));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Closing));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Time_Wait));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Close_Wait));
            Assert.True(Enum.IsDefined(typeof(TcpConnectionState), TcpConnectionState.Last_Ack));
        }
    }
}
