
using System.Threading.Tasks;

namespace Magma.Transport.Tcp.SocketStates
{
    // TCP Connection State Diagram
    //
    //                               +---------+ ---------\      active OPEN  
    //                               |  CLOSED |            \    -----------  
    //   Timeout     /-------------->+---------+<---------\   \   create TCB  
    //   -------   /                   |     ^              \   \  snd SYN    
    //   snd RST /        passive OPEN |     |   CLOSE        \   \           
    //         /          ------------ |     | ----------       \   \         
    //       /             create TCB  |     | delete TCB         \   \       
    //     /                           V     |                      \   \     
    //    |                          +---------+            CLOSE    |    \   
    //    |  rcv RST /-------------->|  LISTEN |          ---------- |     |  
    //    |        /                 +---------+          delete TCB |     |  
    //    |      /        rcv SYN      |     |     SEND              |     |  
    //    |     |        -----------   |     |    -------            |     V  
    //  +---------+      snd SYN,ACK  /       \   snd SYN          +---------+
    //  |         |<-----------------           ------------------>|         |
    //  |   SYN   |                    rcv SYN                     |   SYN   |
    //  |   RCVD  |<-----------------------------------------------|   SENT  |
    //  |         |                    snd ACK                     |         |
    //  |         |------------------           -------------------|         |
    //  +---------+   rcv ACK of SYN  \       /  rcv SYN,ACK       +---------+
    //    |           --------------   |     |   -----------                  
    //    |                  x         |     |     snd ACK                    
    //    |                            V     V                                
    //    |  CLOSE                   +---------+                              
    //    | -------                  |  ESTAB  |                              
    //    | snd FIN                  +---------+                              
    //    |                   CLOSE    |     |    rcv FIN                     
    //    V                  -------   |     |    -------                     
    //  +---------+          snd FIN  /       \   snd ACK          +---------+
    //  |  FIN    |<-----------------           ------------------>|  CLOSE  |
    //  | WAIT-1  |------------------                              |   WAIT  |
    //  +---------+          rcv FIN  \                            +---------+
    //    | rcv ACK of FIN   -------   |                            CLOSE  |  
    //    | --------------   snd ACK   |                           ------- |  
    //    V        x                   V                           snd FIN V  
    //  +---------+                  +---------+                   +---------+
    //  |FINWAIT-2|                  | CLOSING |                   | LAST-ACK|
    //  +---------+                  +---------+                   +---------+
    //    |                rcv ACK of FIN |                 rcv ACK of FIN |  
    //    |  rcv FIN       -------------- |    Timeout=2MSL -------------- |  
    //    |  -------              x       V    ------------        x       V  
    //     \ snd ACK                 +---------+delete TCB         +---------+
    //      ------------------------>|TIME WAIT|------------------>| CLOSED  |
    //                               +---------+                   +---------+


    internal struct ResponderClosed
    {
        ValueTask<ResponderListening> ListenAsync() => default;
    }

    internal struct ResponderListening
    {
        ValueTask<ResponderSynReceived> WaitSynAsync() => default;
    }
    internal struct ResponderSynReceived
    {
        ValueTask<ResponderEstablished> SendSynAckAsync() => default;
    }
    internal struct ResponderEstablished
    {
        ValueTask<ResponderCloseWait> WaitCloseAsync() => default;
    }
    internal struct ResponderCloseWait
    {
        ValueTask<ResponderLastAck> SendFinAsync() => default;
    }
    internal struct ResponderLastAck
    {
        ValueTask<ResponderClosed> WaitFinAsync() => default;
    }

    internal struct InitatorClosed { }
    internal struct InitatorSynSent { }
    internal struct InitatorEstablished { }
    internal struct InitatorFinWait1 { }
    internal struct InitatorFinWait2 { }
    internal struct InitatorClosing { }
    internal struct InitatorTimeWait { }
}
